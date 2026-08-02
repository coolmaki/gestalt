using Supercluster.Lib.Application.Commands;

namespace Passport.Core.Application.Commands;

/// <summary>
/// Generates WebAuthn registration options for the add-passkey-from-new-device flow.
/// Requires a valid <c>AddPasskeyToken</c> obtained from
/// <see cref="VerifyAddPasskeyCodeCommandHandler"/>.
/// <para>
/// Phase B (WebAuthn credential registration) — step 3 of 4 in the add-passkey flow.
/// The client calls <c>navigator.credentials.create()</c> with the returned options,
/// then calls <see cref="CompleteAddPasskeyCommand"/> with the attestation.
/// </para>
/// </summary>
public sealed record BeginAddPasskeyRegistrationCommand(string AddPasskeyToken) : ICommand<BeginAddPasskeyRegistrationResult>;