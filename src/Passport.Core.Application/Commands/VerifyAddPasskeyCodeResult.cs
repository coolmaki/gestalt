namespace Passport.Core.Application.Commands;

/// <summary>
/// Result of verifying the device verification code. Contains the
/// <c>AddPasskeyToken</c> that authorizes WebAuthn credential registration
/// in the subsequent steps of the add-passkey flow.
/// </summary>
public sealed record VerifyAddPasskeyCodeResult(string AddPasskeyToken);