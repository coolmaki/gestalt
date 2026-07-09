using Passport.Core.Application.Configuration;
using Passport.Core.Application.Services;
using Passport.Core.Domain.ValueObjects;
using Supercluster.Lib.Infrastructure.Email;

namespace Passport.Infrastructure.Services;

internal sealed class CodeDeliveryService(IEmailSender emailSender, ApplicationConfiguration appConfig) : ICodeDeliveryService
{
    public Task SendVerificationCodeAsync(Email to, string code, CancellationToken cancellationToken)
    {
        var subject = "Verify your email";
        var body = $"Your verification code is: {code}\n\n{appConfig.BaseUrl}/verify";
        return emailSender.SendAsync(to.Value, subject, body, cancellationToken);
    }

    public Task SendRecoveryCodeAsync(Email to, string code, CancellationToken cancellationToken)
    {
        var subject = "Account recovery code";
        var body = $"Your recovery code is: {code}\n\n{appConfig.BaseUrl}/recover";
        return emailSender.SendAsync(to.Value, subject, body, cancellationToken);
    }
}