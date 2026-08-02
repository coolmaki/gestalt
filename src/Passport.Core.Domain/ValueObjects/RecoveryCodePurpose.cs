namespace Passport.Core.Domain.ValueObjects;

/// <summary>
/// Distinguishes the purpose of a recovery code. Each purpose has its own
/// TTL, rate limits, and email template. The <see cref="DeviceVerification"/>
/// purpose is used in the add-passkey-from-new-device flow.
/// </summary>
public enum RecoveryCodePurpose
{
    /// <summary>
    /// Code sent during initial email verification after registration.
    /// </summary>
    EmailVerification,

    /// <summary>
    /// Code sent during account recovery (lost device / lost passkey).
    /// </summary>
    AccountRecovery,

    /// <summary>
    /// Code sent to verify ownership of an email before adding a new
    /// passkey credential from a device that doesn't have one yet.
    /// This is Phase A (email verification) of the add-passkey flow.
    /// Phase B (WebAuthn credential registration) follows once the code
    /// is verified and an <c>AddPasskeyToken</c> is issued.
    /// </summary>
    DeviceVerification,
}