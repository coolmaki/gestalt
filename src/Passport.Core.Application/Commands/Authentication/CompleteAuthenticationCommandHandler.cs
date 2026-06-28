using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Ports;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands.Authentication;

internal sealed class CompleteAuthenticationCommandHandler : ICommandHandler<CompleteAuthenticationCommand, Unit>
{
    private readonly IUserCommandRepository _userRepo;
    private readonly IChallengeStore _challengeStore;
    private readonly IFido2 _fido2;
    private readonly IDateTimeProvider _clock;

    public CompleteAuthenticationCommandHandler(
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

    public async Task<Result<Unit>> HandleAsync(CompleteAuthenticationCommand command, CancellationToken ct)
    {
        // 1. Validate email
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return emailResult.Error;
        }

        Email email = emailResult.Value;

        // 2. Load user aggregate
        var userOption = await _userRepo.FindByEmailAsync(email, ct);
        if (userOption.IsNone)
        {
            return Error.NotFound("user.not_found", "No user found with this email.");
        }

        User user = userOption.Value;

        // 3. Retrieve challenge
        var challengeOption = await _challengeStore.GetAndRemoveAsync(email.Value, ct);
        if (challengeOption.IsNone)
        {
            return Error.Validation("challenge.expired", "Authentication challenge expired or not found. Please start again.");
        }

        byte[] challenge = challengeOption.Value;

        // 4. Find the credential being used
        // The assertion response contains the credential ID; the handler matches it against the user's passkeys
        var passkeys = user.Passkeys.ToArray();
        if (passkeys.Length == 0)
        {
            return Error.NotFound("credentials.not_found", "No passkeys registered for this user.");
        }

        // 5. Complete assertion against each credential until one matches
        Result<uint>? assertionResult = null;
        PasskeyCredential? matchedCredential = null;

        foreach (var passkey in passkeys)
        {
            assertionResult = await _fido2.CompleteAssertionAsync(challenge, command.AssertionJson, passkey.PublicKey, passkey.SignCount, ct);
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

        // 6. Update sign count
        matchedCredential.UpdateSignCount(assertionResult.Value);

        await _userRepo.SaveChangesAsync(ct);

        return Unit.Value;
    }
}