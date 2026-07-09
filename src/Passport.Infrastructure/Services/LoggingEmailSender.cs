using Microsoft.Extensions.Logging;
using Supercluster.Lib.Infrastructure.Email;

namespace Passport.Infrastructure.Services;

#pragma warning disable CA1848 // Dev logger is intentionally simple
internal sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken)
    {
        logger.LogInformation("Email to {To}: {Subject}", to, subject);
        return Task.CompletedTask;
    }
}
#pragma warning restore CA1848