using Gestalt.Lib.Application.Commands;
using Gestalt.Lib.Application.Providers;
using Gestalt.Lib.Primitives;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands;

/// <summary>
/// Validates the recovery code and issues a short-lived recovery token. The token
/// is bound to the user's email via <see cref="IChallengeStore"/>.
/// </summary>
internal sealed class VerifyRecoveryCodeCommandHandler(
    IRecoveryCodeRepository recoveryCodeRepo,
    IChallengeStore challengeStore,
    IDateTimeProvider clock
) : ICommandHandler<VerifyRecoveryCodeCommand, VerifyRecoveryCodeResult>
{
    public async Task<Result<VerifyRecoveryCodeResult>> HandleAsync(VerifyRecoveryCodeCommand command, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return Error.Validation("email.invalid", "Invalid email.");
        }

        Email email = emailResult.Value;

        DateTimeOffset now = clock.UtcNow();
        var codeOption = await recoveryCodeRepo.FindActiveByEmailAsync(email, RecoveryCodePurpose.AccountRecovery, now, cancellationToken);
        if (codeOption.IsNone)
        {
            return Error.Validation("recovery_code.invalid", "Invalid or expired recovery code.");
        }

        var recoveryCode = codeOption.Value;

        string providedHash = Helpers.HashCode(command.Code);
        if (!string.Equals(providedHash, recoveryCode.CodeHash, StringComparison.Ordinal))
        {
            return Error.Validation("recovery_code.invalid", "Invalid recovery code.");
        }

        var markResult = recoveryCode.MarkUsed(now);
        if (markResult.IsFailure)
        {
            return markResult.Error;
        }

        await recoveryCodeRepo.SaveChangesAsync(cancellationToken);

        string recoveryToken = Convert.ToHexStringLower(Guid.NewGuid().ToByteArray());
        byte[] emailBytes = System.Text.Encoding.UTF8.GetBytes(email.Value);
        await challengeStore.SetAsync($"recovery:{recoveryToken}", emailBytes, TimeSpan.FromMinutes(5), cancellationToken);

        return new VerifyRecoveryCodeResult(recoveryToken);
    }
}