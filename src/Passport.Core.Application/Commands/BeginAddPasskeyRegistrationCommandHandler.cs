using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Services;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands;

/// <summary>
/// Handles <see cref="BeginAddPasskeyRegistrationCommand"/>. Validates the
/// <c>AddPasskeyToken</c>, extracts the email, generates WebAuthn registration
/// options via <see cref="IFido2"/>, and stores the challenge for the subsequent
/// <see cref="CompleteAddPasskeyCommand"/>.
/// <para>
/// Phase B (WebAuthn credential registration) — step 3 of 4 in the add-passkey flow.
/// </para>
/// </summary>
internal sealed class BeginAddPasskeyRegistrationCommandHandler(
    IChallengeStore challengeStore,
    IFido2 fido2
) : ICommandHandler<BeginAddPasskeyRegistrationCommand, BeginAddPasskeyRegistrationResult>
{
    public async Task<Result<BeginAddPasskeyRegistrationResult>> HandleAsync(BeginAddPasskeyRegistrationCommand command, CancellationToken cancellationToken)
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

        var optionsResult = await fido2.CreateRegistrationOptionsAsync(email, cancellationToken);
        if (optionsResult.IsFailure)
        {
            return optionsResult.Error;
        }

        (string optionsJson, string internalState) = optionsResult.Value;

        await challengeStore.SetAsync(email.Value, System.Text.Encoding.UTF8.GetBytes(internalState), TimeSpan.FromMinutes(5), cancellationToken);
        byte[] emailBytes = System.Text.Encoding.UTF8.GetBytes(email.Value);
        await challengeStore.SetAsync($"add-passkey:{command.AddPasskeyToken}", emailBytes, TimeSpan.FromMinutes(5), cancellationToken);

        return new BeginAddPasskeyRegistrationResult(optionsJson);
    }
}