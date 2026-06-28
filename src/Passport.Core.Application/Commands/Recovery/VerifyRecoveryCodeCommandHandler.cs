using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Commands.Registration;
using Passport.Core.Application.Ports;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands.Recovery;

internal sealed class VerifyRecoveryCodeCommandHandler : ICommandHandler<VerifyRecoveryCodeCommand, string>
{
    private readonly IRecoveryCodeRepository _recoveryCodeRepo;
    private readonly IChallengeStore _challengeStore;
    private readonly IDateTimeProvider _clock;

    public VerifyRecoveryCodeCommandHandler(
        IRecoveryCodeRepository recoveryCodeRepo,
        IChallengeStore challengeStore,
        IDateTimeProvider clock)
    {
        _recoveryCodeRepo = recoveryCodeRepo;
        _challengeStore = challengeStore;
        _clock = clock;
    }

    public async Task<Result<string>> HandleAsync(VerifyRecoveryCodeCommand command, CancellationToken ct)
    {
        // 1. Validate email
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return Error.Validation("email.invalid", "Invalid email.");
        }

        Email email = emailResult.Value;

        // 2. Find active recovery code
        DateTimeOffset now = _clock.UtcNow();
        var codeOption = await _recoveryCodeRepo.FindActiveByEmailAsync(email, RecoveryCodePurpose.AccountRecovery, now, ct);
        if (codeOption.IsNone)
        {
            return Error.Validation("recovery_code.invalid", "Invalid or expired recovery code.");
        }

        var recoveryCode = codeOption.Value;

        // 3. Hash the provided code and compare
        string providedHash = CompleteRegistrationCommandHandler.HashCode(command.Code);
        if (!string.Equals(providedHash, recoveryCode.CodeHash, StringComparison.Ordinal))
        {
            return Error.Validation("recovery_code.invalid", "Invalid recovery code.");
        }

        // 4. Mark code as used
        var markResult = recoveryCode.MarkUsed(now);
        if (markResult.IsFailure)
        {
            return markResult.Error;
        }

        await _recoveryCodeRepo.SaveChangesAsync(ct);

        // 5. Issue a short-lived recovery token bound to this email
        string recoveryToken = Convert.ToHexStringLower(Guid.NewGuid().ToByteArray());
        byte[] emailBytes = System.Text.Encoding.UTF8.GetBytes(email.Value);
        await _challengeStore.SetAsync($"recovery:{recoveryToken}", emailBytes, TimeSpan.FromMinutes(5), ct);

        return recoveryToken;
    }
}