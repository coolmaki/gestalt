using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Services;

/// <summary>
/// Delivers verification and recovery codes to users.
/// Application code uses this to request code delivery; infrastructure handles how.
/// </summary>
public interface ICodeDeliveryService
{
    /// <summary>
    /// Sends an email verification code to the user.
    /// </summary>
    Task SendVerificationCodeAsync(Email to, string code, CancellationToken cancellationToken);

    /// <summary>
    /// Sends an account recovery code to the user.
    /// </summary>
    Task SendRecoveryCodeAsync(Email to, string code, CancellationToken cancellationToken);

    /// <summary>
    /// Sends a device verification code to the user for the add-passkey-from-new-device flow.
    /// This is Phase A (email verification) — the user must prove they own the email
    /// before being allowed to add a new passkey to their account.
    /// </summary>
    Task SendDeviceVerificationCodeAsync(Email to, string code, CancellationToken cancellationToken);
}