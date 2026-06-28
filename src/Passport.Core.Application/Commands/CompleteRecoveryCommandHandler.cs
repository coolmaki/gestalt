using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands;

/// <summary>
/// Completes the account recovery flow. Validates the recovery token, removes all
/// existing passkeys, and registers the new passkey from the WebAuthn attestation.
/// </summary>
internal sealed class CompleteRecoveryCommandHandler(
    IUserCommandRepository userRepo,
    IChallengeStore challengeStore,
    IFido2 fido2,
    IDateTimeProvider clock
) : ICommandHandler<CompleteRecoveryCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(CompleteRecoveryCommand command, CancellationToken ct)
    {
        var tokenData = await challengeStore.GetAndRemoveAsync($"recovery:{command.RecoveryToken}", ct);
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

        var userOption = await userRepo.FindByEmailAsync(email, ct);
        if (userOption.IsNone)
        {
            return Error.NotFound("user.not_found", "User not found.");
        }

        User user = userOption.Value;

        var challengeOption = await challengeStore.GetAndRemoveAsync(email.Value, ct);
        if (challengeOption.IsNone)
        {
            return Error.Validation("challenge.expired", "Recovery registration challenge expired. Please start again.");
        }

        byte[] challenge = challengeOption.Value;

        var attestationResult = await fido2.CompleteRegistrationAsync(challenge, command.AttestationJson, ct);
        if (attestationResult.IsFailure)
        {
            return attestationResult.Error;
        }

        (byte[] credentialId, byte[] publicKey, uint signCount) = attestationResult.Value;

        DateTimeOffset now = clock.UtcNow();
        var existingPasskeys = user.Passkeys.ToArray();
        foreach (var pk in existingPasskeys)
        {
            var removeResult = user.RemovePasskey(pk.CredentialId, now);
            if (removeResult.IsFailure)
            {
                return removeResult.Error;
            }
        }

        var passkeyResult = user.AddPasskey(credentialId, publicKey, signCount, now);
        if (passkeyResult.IsFailure)
        {
            return passkeyResult.Error;
        }

        await userRepo.SaveChangesAsync(ct);

        return Unit.Value;
    }
}