using Gestalt.Lib.Application.Commands;
using Gestalt.Lib.Application.Providers;
using Gestalt.Lib.Primitives;
using Passport.Core.Application.Repositories;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands;

internal sealed class VerifyEmailCommandHandler(
    IUserCommandRepository userRepo,
    IRecoveryCodeRepository recoveryCodeRepo,
    IDateTimeProvider clock
) : ICommandHandler<VerifyEmailCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return emailResult.Error;
        }

        Email email = emailResult.Value;

        var userOption = await userRepo.FindByEmailAsync(email, cancellationToken);
        if (userOption.IsNone)
        {
            return Error.NotFound("user.not_found", "User not found.");
        }

        User user = userOption.Value;

        DateTimeOffset now = clock.UtcNow();
        var codeOption = await recoveryCodeRepo.FindActiveByEmailAsync(email, RecoveryCodePurpose.EmailVerification, now, cancellationToken);
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

        var verifyResult = user.VerifyEmail(now);
        if (verifyResult.IsFailure)
        {
            return verifyResult.Error;
        }

        await userRepo.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}