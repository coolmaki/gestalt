using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Ports;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands.Recovery;

internal sealed class BeginRecoveryRegistrationCommandHandler : ICommandHandler<BeginRecoveryRegistrationCommand, string>
{
    private readonly IChallengeStore _challengeStore;
    private readonly IFido2 _fido2;

    public BeginRecoveryRegistrationCommandHandler(IChallengeStore challengeStore, IFido2 fido2)
    {
        _challengeStore = challengeStore;
        _fido2 = fido2;
    }

    public async Task<Result<string>> HandleAsync(BeginRecoveryRegistrationCommand command, CancellationToken ct)
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

        // 2. Generate WebAuthn registration options for the new passkey
        var optionsResult = await _fido2.CreateRegistrationOptionsAsync(email, ct);
        if (optionsResult.IsFailure)
        {
            return optionsResult.Error;
        }

        (string optionsJson, byte[] challenge) = optionsResult.Value;

        // 3. Store challenge and re-store the email-recovery binding for the completion step
        await _challengeStore.SetAsync(email.Value, challenge, TimeSpan.FromMinutes(5), ct);
        byte[] emailBytes = System.Text.Encoding.UTF8.GetBytes(email.Value);
        await _challengeStore.SetAsync($"recovery:{command.RecoveryToken}", emailBytes, TimeSpan.FromMinutes(5), ct);

        return optionsJson;
    }
}