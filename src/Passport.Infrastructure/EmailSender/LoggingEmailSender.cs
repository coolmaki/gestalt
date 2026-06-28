using Microsoft.Extensions.Logging;
using Passport.Core.Application.Services;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Infrastructure.EmailSender;

#pragma warning disable CA1848 // Dev logger is intentionally simple
internal sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendVerificationCodeAsync(Email to, string code, CancellationToken cancellationToken)
    {
        logger.LogInformation("Verification code for {Email}: {Code}", to.Value, code);
        return Task.CompletedTask;
    }

    public Task SendRecoveryCodeAsync(Email to, string code, CancellationToken cancellationToken)
    {
        logger.LogInformation("Recovery code for {Email}: {Code}", to.Value, code);
        return Task.CompletedTask;
    }
}
#pragma warning restore CA1848