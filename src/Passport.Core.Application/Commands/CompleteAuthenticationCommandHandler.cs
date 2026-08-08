using Gestalt.Lib.Application.Commands;
using Gestalt.Lib.Application.Providers;
using Gestalt.Lib.Primitives;
using Passport.Core.Application.Configuration;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands;

/// <summary>
/// Completes the authentication flow. Validates the WebAuthn assertion against
/// the user's registered passkeys. Tries each credential until one matches.
/// Updates the sign count on success and returns a session (access token + refresh token).
/// </summary>
internal sealed class CompleteAuthenticationCommandHandler(
    IUserCommandRepository userRepo,
    IChallengeStore challengeStore,
    IFido2 fido2,
    ITokenService tokenService,
    IDateTimeProvider clock,
    ApplicationConfiguration appConfig
) : ICommandHandler<CompleteAuthenticationCommand, SessionResult>
{
    public async Task<Result<SessionResult>> HandleAsync(CompleteAuthenticationCommand command, CancellationToken cancellationToken)
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

        string internalState = System.Text.Encoding.UTF8.GetString(challengeOption.Value);

        var passkeys = user.Passkeys.ToArray();
        if (passkeys.Length == 0)
        {
            return Error.NotFound("credentials.not_found", "No passkeys registered for this user.");
        }

        Result<uint>? assertionResult = null;
        PasskeyCredential? matchedCredential = null;

        foreach (var passkey in passkeys)
        {
            assertionResult = await fido2.CompleteAssertionAsync(internalState, command.AssertionJson, passkey.PublicKey, passkey.SignCount, cancellationToken);
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

        var now = clock.UtcNow();

        var accessToken = tokenService.GenerateAccessToken(user.Email.Value);

        var (rawToken, tokenHash) = tokenService.GenerateRefreshToken();
        var refreshTtl = TimeSpan.FromDays(appConfig.RefreshToken.LifetimeDays);

        user.IssueRefreshToken(tokenHash, now, refreshTtl);

        await userRepo.SaveChangesAsync(cancellationToken);

        var result = new SessionResult(
            accessToken,
            rawToken,
            appConfig.AccessToken.LifetimeMinutes * 60
        );

        return result;
    }
}