using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Primitives;

namespace Passport.Core.Application.Commands;

/// <summary>
/// Completes the add-passkey-from-new-device flow. Validates the WebAuthn
/// attestation and adds the new passkey credential to the user's account.
/// Unlike account recovery, this does NOT remove existing passkeys — it
/// adds the new one alongside them.
/// <para>
/// Phase B (WebAuthn credential registration) — step 4 of 4 in the add-passkey flow.
/// </para>
/// </summary>
public sealed record CompleteAddPasskeyCommand(string AddPasskeyToken, string AttestationJson) : ICommand<Unit>;