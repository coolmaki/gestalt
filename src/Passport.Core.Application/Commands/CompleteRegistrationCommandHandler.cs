using System.Security.Cryptography;
using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Ports.Repositories;
using Passport.Core.Application.Ports.Services;
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

    public async Task<Result<Unit>> HandleAsync(CompleteRegistrationCommand command, CancellationToken ct)
    {
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return emailResult.Error;
        }

        Email email = emailResult.Value;

        var existing = await userQueryRepo.FindByEmailAsync(email.Value, ct);
        if (existing.IsSome)
        {
            return Error.Conflict("email.already_registered", "A user with this email already exists.");
        }

        var challengeOption = await challengeStore.GetAndRemoveAsync(email.Value, ct);
        if (challengeOption.IsNone)
        {
            return Error.Validation("challenge.expired", "Registration challenge expired or not found. Please start again.");
        }

        byte[] challenge = challengeOption.Value;

        var attestationResult = await fido2.CompleteRegistrationAsync(challenge, command.AttestationJson, ct);
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
        var recoveryCodeResult = RecoveryCode.Issue(recoveryCodeId, codeHash, RecoveryCodePurpose.EmailVerification, now, VerificationCodeTtl);
        if (recoveryCodeResult.IsFailure)
        {
            return recoveryCodeResult.Error;
        }

        await userRepo.AddAsync(user, ct);
        await recoveryCodeRepo.AddAsync(recoveryCodeResult.Value, ct);
        await userRepo.SaveChangesAsync(ct);

        await emailSender.SendVerificationCodeAsync(email, code, ct);

        return Unit.Value;
    }
}