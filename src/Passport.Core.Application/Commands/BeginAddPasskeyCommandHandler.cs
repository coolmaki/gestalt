using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands;

/// <summary>
/// Handles <see cref="BeginAddPasskeyCommand"/>. Sends a 6-digit verification code
/// to the user's verified email. Silently succeeds if the email is not registered
/// or not verified — no user enumeration.
/// <para>
/// Phase A (email verification) — step 1 of 4 in the add-passkey flow.
/// </para>
/// </summary>
internal sealed class BeginAddPasskeyCommandHandler(
    IUserQueryRepository userQueryRepo,
    IRecoveryCodeRepository recoveryCodeRepo,
    ICodeDeliveryService codeDelivery,
    IGuidProvider guids,
    IDateTimeProvider clock
) : ICommandHandler<BeginAddPasskeyCommand, Unit>
{
    private static readonly TimeSpan CodeTtl = TimeSpan.FromMinutes(10);

    public async Task<Result<Unit>> HandleAsync(BeginAddPasskeyCommand command, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return Unit.Value;
        }

        Email email = emailResult.Value;

        var userOption = await userQueryRepo.FindByEmailAsync(email.Value, cancellationToken);
        if (userOption.IsNone || userOption.Value.EmailVerified == 0)
        {
            return Unit.Value;
        }

        DateTimeOffset now = clock.UtcNow();
        string code = Helpers.GenerateCode();
        string codeHash = Helpers.HashCode(code);
        var codeId = new RecoveryCodeId(guids.NewGuid());

        var codeResult = RecoveryCode.Issue(codeId, email, codeHash, RecoveryCodePurpose.DeviceVerification, now, CodeTtl);
        if (codeResult.IsFailure)
        {
            return codeResult.Error;
        }

        await recoveryCodeRepo.AddAsync(codeResult.Value, cancellationToken);
        await recoveryCodeRepo.SaveChangesAsync(cancellationToken);

        await codeDelivery.SendDeviceVerificationCodeAsync(email, code, cancellationToken);

        return Unit.Value;
    }
}