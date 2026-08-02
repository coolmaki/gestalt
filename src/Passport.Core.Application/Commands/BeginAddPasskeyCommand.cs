using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Primitives;

namespace Passport.Core.Application.Commands;

/// <summary>
/// Begins the add-passkey-from-new-device flow. Sends a 6-digit verification code
/// to the user's verified email. Silently succeeds if the email is not registered
/// or not verified — no user enumeration.
/// <para>
/// This is Phase A (email verification) of the add-passkey flow. Once the code is
/// verified, the caller receives an <c>AddPasskeyToken</c> and proceeds to
/// Phase B (WebAuthn credential registration) via
/// <see cref="BeginAddPasskeyRegistrationCommand"/> and
/// <see cref="CompleteAddPasskeyCommand"/>.
/// </para>
/// </summary>
public sealed record BeginAddPasskeyCommand(string Email) : ICommand<Unit>;