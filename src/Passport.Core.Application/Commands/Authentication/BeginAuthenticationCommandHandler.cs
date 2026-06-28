using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Ports;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands.Authentication;

internal sealed class BeginAuthenticationCommandHandler : ICommandHandler<BeginAuthenticationCommand, string>
{
    private readonly IUserQueryRepository _userQueryRepo;
    private readonly IFido2 _fido2;
    private readonly IChallengeStore _challengeStore;

    public BeginAuthenticationCommandHandler(IUserQueryRepository userQueryRepo, IFido2 fido2, IChallengeStore challengeStore)
    {
        _userQueryRepo = userQueryRepo;
        _fido2 = fido2;
        _challengeStore = challengeStore;
    }

    public async Task<Result<string>> HandleAsync(BeginAuthenticationCommand command, CancellationToken ct)
    {
        // 1. Validate email
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return emailResult.Error;
        }

        Email email = emailResult.Value;

        // 2. Look up user
        var userOption = await _userQueryRepo.FindByEmailAsync(email.Value, ct);
        if (userOption.IsNone)
        {
            return Error.NotFound("user.not_found", "No user found with this email.");
        }

        // 3. Get allowed credentials
        var credentials = await _userQueryRepo.GetCredentialsAsync(email.Value, ct);
        if (credentials.Count == 0)
        {
            return Error.NotFound("credentials.not_found", "No passkeys registered for this user.");
        }

        byte[][] allowedCredentialIds = credentials.Select(c => c.CredentialId).ToArray();

        // 4. Generate assertion options
        var optionsResult = await _fido2.CreateAssertionOptionsAsync(allowedCredentialIds, ct);
        if (optionsResult.IsFailure)
        {
            return optionsResult.Error;
        }

        (string optionsJson, byte[] challenge) = optionsResult.Value;

        // 5. Store challenge
        await _challengeStore.SetAsync(email.Value, challenge, TimeSpan.FromMinutes(5), ct);

        return optionsJson;
    }
}