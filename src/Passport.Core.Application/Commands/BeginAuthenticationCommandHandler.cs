using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands;

/// <summary>
/// First step of the authentication flow. Generates WebAuthn assertion options
/// for the user's registered passkeys. The client calls
/// <c>navigator.credentials.get()</c> with the returned options, then calls
/// <see cref="CompleteAuthenticationCommandHandler"/>.
/// </summary>
internal sealed class BeginAuthenticationCommandHandler(
    IUserQueryRepository userQueryRepo,
    IFido2 fido2,
    IChallengeStore challengeStore
) : ICommandHandler<BeginAuthenticationCommand, BeginAuthenticationResult>
{
    public async Task<Result<BeginAuthenticationResult>> HandleAsync(BeginAuthenticationCommand command, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return emailResult.Error;
        }

        Email email = emailResult.Value;

        var userOption = await userQueryRepo.FindByEmailAsync(email.Value, cancellationToken);
        if (userOption.IsNone)
        {
            return Error.NotFound("user.not_found", "No user found with this email.");
        }

        if (userOption.Value.EmailVerified == 0)
        {
            return Error.Unauthorized("email.not_verified", "Email not verified. Please verify your email before authenticating.");
        }

        var credentials = await userQueryRepo.GetCredentialsAsync(email.Value, cancellationToken);
        if (credentials.Count == 0)
        {
            return Error.NotFound("credentials.not_found", "No passkeys registered for this user.");
        }

        byte[][] allowedCredentialIds = credentials.Select(c => c.CredentialId).ToArray();

        var optionsResult = await fido2.CreateAssertionOptionsAsync(allowedCredentialIds, cancellationToken);
        if (optionsResult.IsFailure)
        {
            return optionsResult.Error;
        }

        (string optionsJson, string internalState) = optionsResult.Value;

        await challengeStore.SetAsync(email.Value, System.Text.Encoding.UTF8.GetBytes(internalState), TimeSpan.FromMinutes(5), cancellationToken);

        return new BeginAuthenticationResult(optionsJson);
    }
}