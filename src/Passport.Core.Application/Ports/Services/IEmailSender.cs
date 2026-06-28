using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Ports.Services;

/// <summary>
/// Sends transactional emails (verification codes, recovery codes).
/// Infrastructure provides SMTP, dev provides a console logger.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email verification code to the user.
    /// </summary>
    Task SendVerificationCodeAsync(Email to, string code, CancellationToken ct);

    /// <summary>
    /// Sends an account recovery code to the user.
    /// </summary>
    Task SendRecoveryCodeAsync(Email to, string code, CancellationToken ct);
}