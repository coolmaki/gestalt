using System.Security.Cryptography;
using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands;

internal sealed class CompleteRegistrationCommandHandler(
    IUserCommandRepository userRepo,
    IUserQueryRepository userQueryRepo,
    IRecoveryCodeRepository recoveryCodeRepo,
    IChallengeStore challengeStore,
    IFido2 fido2,
    IEmailSender emailSender,
    IGuidProvider guids,
    IDateTimeProvider clock
) : ICommandHandler<CompleteRegistrationCommand, Unit>
{
    private static readonly TimeSpan VerificationCodeTtl = TimeSpan.FromMinutes(10);

    public async Task<Result<Unit>> HandleAsync(CompleteRegistrationCommand command, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return emailResult.Error;
        }

        Email email = emailResult.Value;

        var existing = await userQueryRepo.FindByEmailAsync(email.Value, cancellationToken);
        if (existing.IsSome)
        {
            return Error.Conflict("email.already_registered", "A user with this email already exists.");
        }

        var challengeOption = await challengeStore.GetAndRemoveAsync(email.Value, cancellationToken);
        if (challengeOption.IsNone)
        {
            return Error.Validation("challenge.expired", "Registration challenge expired or not found. Please start again.");
        }

        byte[] challenge = challengeOption.Value;

        var attestationResult = await fido2.CompleteRegistrationAsync(challenge, command.AttestationJson, cancellationToken);
        if (attestationResult.IsFailure)
        {
            return attestationResult.Error;
        }

        (byte[] credentialId, byte[] publicKey, uint signCount) = attestationResult.Value;

        DateTimeOffset now = clock.UtcNow();
        var userResult = User.Register(email, now);
        if (userResult.IsFailure)
        {
            return userResult.Error;
        }

        User user = userResult.Value;

        var passkeyResult = user.AddPasskey(credentialId, publicKey, signCount, now);
        if (passkeyResult.IsFailure)
        {
            return passkeyResult.Error;
        }

        string code = Helpers.GenerateCode();
        string codeHash = Helpers.HashCode(code);
        var recoveryCodeId = new RecoveryCodeId(guids.NewGuid());
        var recoveryCodeResult = RecoveryCode.Issue(recoveryCodeId, email, codeHash, RecoveryCodePurpose.EmailVerification, now, VerificationCodeTtl);
        if (recoveryCodeResult.IsFailure)
        {
            return recoveryCodeResult.Error;
        }

        await userRepo.AddAsync(user, cancellationToken);
        await recoveryCodeRepo.AddAsync(recoveryCodeResult.Value, cancellationToken);
        await userRepo.SaveChangesAsync(cancellationToken);

        await emailSender.SendVerificationCodeAsync(email, code, cancellationToken);

        return Unit.Value;
    }
}