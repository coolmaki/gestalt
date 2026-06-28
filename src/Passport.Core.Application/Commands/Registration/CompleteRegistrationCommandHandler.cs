using System.Security.Cryptography;
using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Ports;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands.Registration;

internal sealed class CompleteRegistrationCommandHandler : ICommandHandler<CompleteRegistrationCommand, Unit>
{
    private readonly IUserCommandRepository _userRepo;
    private readonly IUserQueryRepository _userQueryRepo;
    private readonly IRecoveryCodeRepository _recoveryCodeRepo;
    private readonly IChallengeStore _challengeStore;
    private readonly IFido2 _fido2;
    private readonly IEmailSender _emailSender;
    private readonly IGuidProvider _guids;
    private readonly IDateTimeProvider _clock;

    private static readonly TimeSpan VerificationCodeTtl = TimeSpan.FromMinutes(10);

    public CompleteRegistrationCommandHandler(
        IUserCommandRepository userRepo,
        IUserQueryRepository userQueryRepo,
        IRecoveryCodeRepository recoveryCodeRepo,
        IChallengeStore challengeStore,
        IFido2 fido2,
        IEmailSender emailSender,
        IGuidProvider guids,
        IDateTimeProvider clock)
    {
        _userRepo = userRepo;
        _userQueryRepo = userQueryRepo;
        _recoveryCodeRepo = recoveryCodeRepo;
        _challengeStore = challengeStore;
        _fido2 = fido2;
        _emailSender = emailSender;
        _guids = guids;
        _clock = clock;
    }

    public async Task<Result<Unit>> HandleAsync(CompleteRegistrationCommand command, CancellationToken ct)
    {
        // 1. Validate email
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return emailResult.Error;
        }

        Email email = emailResult.Value;

        // 2. Check uniqueness (race-safe re-check)
        var existing = await _userQueryRepo.FindByEmailAsync(email.Value, ct);
        if (existing.IsSome)
        {
            return Error.Conflict("email.already_registered", "A user with this email already exists.");
        }

        // 3. Retrieve and remove challenge
        var challengeOption = await _challengeStore.GetAndRemoveAsync(email.Value, ct);
        if (challengeOption.IsNone)
        {
            return Error.Validation("challenge.expired", "Registration challenge expired or not found. Please start again.");
        }

        byte[] challenge = challengeOption.Value;

        // 4. Complete WebAuthn registration
        var attestationResult = await _fido2.CompleteRegistrationAsync(challenge, command.AttestationJson, ct);
        if (attestationResult.IsFailure)
        {
            return attestationResult.Error;
        }

        (byte[] credentialId, byte[] publicKey, uint signCount) = attestationResult.Value;

        // 5. Create User
        DateTimeOffset now = _clock.UtcNow();
        var userResult = User.Register(email, now);
        if (userResult.IsFailure)
        {
            return userResult.Error;
        }

        User user = userResult.Value;

        // 6. Add the passkey
        var passkeyResult = user.AddPasskey(credentialId, publicKey, signCount, now);
        if (passkeyResult.IsFailure)
        {
            return passkeyResult.Error;
        }

        // 7. Generate verification code and persist
        string code = GenerateCode();
        string codeHash = HashCode(code);
        var recoveryCodeId = new RecoveryCodeId(_guids.NewGuid());
        var recoveryCodeResult = RecoveryCode.Issue(recoveryCodeId, codeHash, RecoveryCodePurpose.EmailVerification, now, VerificationCodeTtl);
        if (recoveryCodeResult.IsFailure)
        {
            return recoveryCodeResult.Error;
        }

        // 8. Persist
        await _userRepo.AddAsync(user, ct);
        await _recoveryCodeRepo.AddAsync(recoveryCodeResult.Value, ct);
        await _userRepo.SaveChangesAsync(ct);

        // 9. Send verification email (fire-and-forget after persist)
        await _emailSender.SendVerificationCodeAsync(email, code, ct);

        return Unit.Value;
    }

    // ------------------------------------------------------------
    // Internal
    // ------------------------------------------------------------

    internal static string GenerateCode()
    {
        return RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6", System.Globalization.CultureInfo.InvariantCulture);
    }

    internal static string HashCode(string code)
    {
        byte[] hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(code));
        return Convert.ToHexStringLower(hash);
    }
}