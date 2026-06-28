using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands;

/// <summary>
/// Completes the authentication flow. Validates the WebAuthn assertion against
/// the user's registered passkeys. Tries each credential until one matches.
/// Updates the sign count on success.
/// </summary>
internal sealed class CompleteAuthenticationCommandHandler(
    IUserCommandRepository userRepo,
    IChallengeStore challengeStore,
    IFido2 fido2
) : ICommandHandler<CompleteAuthenticationCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(CompleteAuthenticationCommand command, CancellationToken cancellationToken)
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
            return Error.NotFound("user.not_found", "No user found with this email.");
        }

        User user = userOption.Value;

        var challengeOption = await challengeStore.GetAndRemoveAsync(email.Value, cancellationToken);
        if (challengeOption.IsNone)
        {
            return Error.Validation("challenge.expired", "Authentication challenge expired or not found. Please start again.");
        }

        byte[] challenge = challengeOption.Value;

        var passkeys = user.Passkeys.ToArray();
        if (passkeys.Length == 0)
        {
            return Error.NotFound("credentials.not_found", "No passkeys registered for this user.");
        }

        Result<uint>? assertionResult = null;
        PasskeyCredential? matchedCredential = null;

        foreach (var passkey in passkeys)
        {
            assertionResult = await fido2.CompleteAssertionAsync(challenge, command.AssertionJson, passkey.PublicKey, passkey.SignCount, cancellationToken);
            if (assertionResult.IsSuccess)
            {
                matchedCredential = passkey;
                break;
            }
        }

        if (matchedCredential is null || assertionResult is null || assertionResult.IsFailure)
        {
            return Error.Validation("assertion.invalid", "Authentication failed. Invalid passkey assertion.");
        }

        matchedCredential.UpdateSignCount(assertionResult.Value);

        await userRepo.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}