using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Commands.Registration;
using Passport.Core.Application.Ports;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands.Verification;

internal sealed class VerifyEmailCommandHandler : ICommandHandler<VerifyEmailCommand, Unit>
{
    private readonly IUserCommandRepository _userRepo;
    private readonly IRecoveryCodeRepository _recoveryCodeRepo;
    private readonly IDateTimeProvider _clock;

    public VerifyEmailCommandHandler(
        IUserCommandRepository userRepo,
        IRecoveryCodeRepository recoveryCodeRepo,
        IDateTimeProvider clock)
    {
        _userRepo = userRepo;
        _recoveryCodeRepo = recoveryCodeRepo;
        _clock = clock;
    }

    public async Task<Result<Unit>> HandleAsync(VerifyEmailCommand command, CancellationToken ct)
    {
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return emailResult.Error;
        }

        Email email = emailResult.Value;

        // Load user
        var userOption = await _userRepo.FindByEmailAsync(email, ct);
        if (userOption.IsNone)
        {
            return Error.NotFound("user.not_found", "User not found.");
        }

        User user = userOption.Value;

        // Find active verification code
        DateTimeOffset now = _clock.UtcNow();
        var codeOption = await _recoveryCodeRepo.FindActiveByEmailAsync(email, RecoveryCodePurpose.EmailVerification, now, ct);
        if (codeOption.IsNone)
        {
            return Error.Validation("verification_code.invalid", "Invalid or expired verification code.");
        }

        var recoveryCode = codeOption.Value;

        // Compare hash
        string providedHash = CompleteRegistrationCommandHandler.HashCode(command.Code);
        if (!string.Equals(providedHash, recoveryCode.CodeHash, StringComparison.Ordinal))
        {
            return Error.Validation("verification_code.invalid", "Invalid verification code.");
        }

        // Mark used
        var markResult = recoveryCode.MarkUsed(now);
        if (markResult.IsFailure)
        {
            return markResult.Error;
        }

        // Verify email
        var verifyResult = user.VerifyEmail(now);
        if (verifyResult.IsFailure)
        {
            return verifyResult.Error;
        }

        await _userRepo.SaveChangesAsync(ct);

        return Unit.Value;
    }
}