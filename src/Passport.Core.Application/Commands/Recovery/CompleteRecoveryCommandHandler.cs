using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Ports;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands.Recovery;

internal sealed class CompleteRecoveryCommandHandler : ICommandHandler<CompleteRecoveryCommand, Unit>
{
    private readonly IUserCommandRepository _userRepo;
    private readonly IChallengeStore _challengeStore;
    private readonly IFido2 _fido2;
    private readonly IDateTimeProvider _clock;

    public CompleteRecoveryCommandHandler(
        IUserCommandRepository userRepo,
        IChallengeStore challengeStore,
        IFido2 fido2,
        IDateTimeProvider clock)
    {
        _userRepo = userRepo;
        _challengeStore = challengeStore;
        _fido2 = fido2;
        _clock = clock;
    }

    public async Task<Result<Unit>> HandleAsync(CompleteRecoveryCommand command, CancellationToken ct)
    {
        // 1. Validate recovery token and retrieve email
        var tokenData = await _challengeStore.GetAndRemoveAsync($"recovery:{command.RecoveryToken}", ct);
        if (tokenData.IsNone)
        {
            return Error.Validation("recovery_token.invalid", "Invalid or expired recovery token.");
        }

        string emailValue = System.Text.Encoding.UTF8.GetString(tokenData.Value);

        var emailResult = Email.Create(emailValue);
        if (emailResult.IsFailure)
        {
            return emailResult.Error;
        }

        Email email = emailResult.Value;

        // 2. Load user
        var userOption = await _userRepo.FindByEmailAsync(email, ct);
        if (userOption.IsNone)
        {
            return Error.NotFound("user.not_found", "User not found.");
        }

        User user = userOption.Value;

        // 3. Retrieve WebAuthn challenge (stored by BeginRecoveryRegistration)
        var challengeOption = await _challengeStore.GetAndRemoveAsync(email.Value, ct);
        if (challengeOption.IsNone)
        {
            return Error.Validation("challenge.expired", "Recovery registration challenge expired. Please start again.");
        }

        byte[] challenge = challengeOption.Value;

        // 4. Complete WebAuthn attestation
        var attestationResult = await _fido2.CompleteRegistrationAsync(challenge, command.AttestationJson, ct);
        if (attestationResult.IsFailure)
        {
            return attestationResult.Error;
        }

        (byte[] credentialId, byte[] publicKey, uint signCount) = attestationResult.Value;

        // 5. Remove all existing passkeys
        DateTimeOffset now = _clock.UtcNow();
        var existingPasskeys = user.Passkeys.ToArray();
        foreach (var pk in existingPasskeys)
        {
            var removeResult = user.RemovePasskey(pk.CredentialId, now);
            if (removeResult.IsFailure)
            {
                return removeResult.Error;
            }
        }

        // 6. Add the new passkey
        var passkeyResult = user.AddPasskey(credentialId, publicKey, signCount, now);
        if (passkeyResult.IsFailure)
        {
            return passkeyResult.Error;
        }

        await _userRepo.SaveChangesAsync(ct);

        return Unit.Value;
    }
}