using Gestalt.Lib.Application.Commands;
using Gestalt.Lib.Application.Providers;
using Gestalt.Lib.Primitives;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands;

/// <summary>
/// Handles <see cref="CompleteAddPasskeyCommand"/>. Validates the
/// <c>AddPasskeyToken</c> and the WebAuthn attestation, then adds the new
/// passkey credential to the user's account. Unlike the recovery flow
/// (<see cref="CompleteRecoveryCommandHandler"/>), this does NOT remove
/// existing passkeys — it preserves all existing credentials and adds the
/// new one alongside them.
/// <para>
/// Phase B (WebAuthn credential registration) — step 4 of 4 in the add-passkey flow.
/// </para>
/// </summary>
internal sealed class CompleteAddPasskeyCommandHandler(
    IUserCommandRepository userRepo,
    IChallengeStore challengeStore,
    IFido2 fido2,
    IDateTimeProvider clock
) : ICommandHandler<CompleteAddPasskeyCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(CompleteAddPasskeyCommand command, CancellationToken cancellationToken)
    {
        var tokenData = await challengeStore.GetAndRemoveAsync($"add-passkey:{command.AddPasskeyToken}", cancellationToken);
        if (tokenData.IsNone)
        {
            return Error.Validation("add_passkey_token.invalid", "Invalid or expired add-passkey token.");
        }

        string emailValue = System.Text.Encoding.UTF8.GetString(tokenData.Value);

        var emailResult = Email.Create(emailValue);
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

        var challengeOption = await challengeStore.GetAndRemoveAsync(email.Value, cancellationToken);
        if (challengeOption.IsNone)
        {
            return Error.Validation("challenge.expired", "Registration challenge expired. Please start again.");
        }

        string internalState = System.Text.Encoding.UTF8.GetString(challengeOption.Value);

        var attestationResult = await fido2.CompleteRegistrationAsync(internalState, command.AttestationJson, cancellationToken);
        if (attestationResult.IsFailure)
        {
            return attestationResult.Error;
        }

        (byte[] credentialId, byte[] publicKey, uint signCount) = attestationResult.Value;

        DateTimeOffset now = clock.UtcNow();

        // Unlike the recovery flow, we do NOT remove existing passkeys.
        // This flow is for adding a new passkey from a new device while
        // keeping all existing credentials intact.
        var passkeyResult = user.AddPasskey(credentialId, publicKey, signCount, now);
        if (passkeyResult.IsFailure)
        {
            return passkeyResult.Error;
        }

        await userRepo.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}