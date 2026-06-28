using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Ports.Services;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands;

/// <summary>
/// Generates WebAuthn registration options for the recovery flow. The client
/// calls <c>navigator.credentials.create()</c> with the returned options, then
/// calls <see cref="CompleteRecoveryCommandHandler"/> with the attestation.
/// </summary>
internal sealed class BeginRecoveryRegistrationCommandHandler(
    IChallengeStore challengeStore,
    IFido2 fido2
) : ICommandHandler<BeginRecoveryRegistrationCommand, BeginRecoveryRegistrationResult>
{
    public async Task<Result<BeginRecoveryRegistrationResult>> HandleAsync(BeginRecoveryRegistrationCommand command, CancellationToken ct)
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

        var optionsResult = await fido2.CreateRegistrationOptionsAsync(email, ct);
        if (optionsResult.IsFailure)
        {
            return optionsResult.Error;
        }

        (string optionsJson, byte[] challenge) = optionsResult.Value;

        await challengeStore.SetAsync(email.Value, challenge, TimeSpan.FromMinutes(5), ct);
        byte[] emailBytes = System.Text.Encoding.UTF8.GetBytes(email.Value);
        await challengeStore.SetAsync($"recovery:{command.RecoveryToken}", emailBytes, TimeSpan.FromMinutes(5), ct);

        return new BeginRecoveryRegistrationResult(optionsJson);
    }
}