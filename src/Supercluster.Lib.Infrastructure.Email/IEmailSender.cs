namespace Supercluster.Lib.Infrastructure.Email;

/// <summary>
/// Raw email-sending contract. Transports a pre-formatted email to a recipient.
/// Infrastructure provides SMTP; dev provides a console logger.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email with the given subject and body to the recipient.
    /// </summary>
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken);
}