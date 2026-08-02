using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands;

/// <summary>
/// Handles <see cref="VerifyAddPasskeyCodeCommand"/>. Validates the device
/// verification code and issues a short-lived <c>AddPasskeyToken</c>. The token
/// is bound to the user's email via <see cref="IChallengeStore"/> and authorizes
/// WebAuthn credential registration in the subsequent steps.
/// <para>
/// Phase A (email verification) — step 2 of 4 in the add-passkey flow.
/// </para>
/// </summary>
internal sealed class VerifyAddPasskeyCodeCommandHandler(
    IRecoveryCodeRepository recoveryCodeRepo,
    IChallengeStore challengeStore,
    IDateTimeProvider clock
) : ICommandHandler<VerifyAddPasskeyCodeCommand, VerifyAddPasskeyCodeResult>
{
    public async Task<Result<VerifyAddPasskeyCodeResult>> HandleAsync(VerifyAddPasskeyCodeCommand command, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return Error.Validation("email.invalid", "Invalid email.");
        }

        Email email = emailResult.Value;

        DateTimeOffset now = clock.UtcNow();
        var codeOption = await recoveryCodeRepo.FindActiveByEmailAsync(email, RecoveryCodePurpose.DeviceVerification, now, cancellationToken);
        if (codeOption.IsNone)
        {
            return Error.Validation("verification_code.invalid", "Invalid or expired verification code.");
        }

        var recoveryCode = codeOption.Value;

        string providedHash = Helpers.HashCode(command.Code);
        if (!string.Equals(providedHash, recoveryCode.CodeHash, StringComparison.Ordinal))
        {
            return Error.Validation("verification_code.invalid", "Invalid verification code.");
        }

        var markResult = recoveryCode.MarkUsed(now);
        if (markResult.IsFailure)
        {
            return markResult.Error;
        }

        await recoveryCodeRepo.SaveChangesAsync(cancellationToken);

        string addPasskeyToken = Convert.ToHexStringLower(Guid.NewGuid().ToByteArray());
        byte[] emailBytes = System.Text.Encoding.UTF8.GetBytes(email.Value);
        await challengeStore.SetAsync($"add-passkey:{addPasskeyToken}", emailBytes, TimeSpan.FromMinutes(5), cancellationToken);

        return new VerifyAddPasskeyCodeResult(addPasskeyToken);
    }
}