using Passport.Core.Application.Configuration;
using Passport.Core.Application.Services;
using Passport.Core.Domain.ValueObjects;
using Gestalt.Lib.Infrastructure.Email;

namespace Passport.Infrastructure.Services;

internal sealed class CodeDeliveryService(IEmailSender emailSender, ApplicationConfiguration appConfig) : ICodeDeliveryService
{
    public Task SendVerificationCodeAsync(Email to, string code, CancellationToken cancellationToken)
    {
#if DEBUG
        Console.WriteLine($"--- VERIFICATION CODE for {to.Value}: {code} ---");
#endif
        var subject = "Verify your email";
        var body = $"Your verification code is: {code}\n\n{appConfig.BaseUrl}/verify";
        return emailSender.SendAsync(to.Value, subject, body, cancellationToken);
    }

    public Task SendRecoveryCodeAsync(Email to, string code, CancellationToken cancellationToken)
    {
#if DEBUG
        Console.WriteLine($"--- RECOVERY CODE for {to.Value}: {code} ---");
#endif
        var subject = "Account recovery code";
        var body = $"Your recovery code is: {code}\n\n{appConfig.BaseUrl}/recover";
        return emailSender.SendAsync(to.Value, subject, body, cancellationToken);
    }

    public Task SendDeviceVerificationCodeAsync(Email to, string code, CancellationToken cancellationToken)
    {
#if DEBUG
        Console.WriteLine($"--- DEVICE VERIFICATION CODE for {to.Value}: {code} ---");
#endif
        var subject = "Verify your new device";
        var body = $"Your verification code is: {code}\n\n{appConfig.BaseUrl}/verify-device";
        return emailSender.SendAsync(to.Value, subject, body, cancellationToken);
    }
}