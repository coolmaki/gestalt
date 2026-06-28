using Microsoft.Extensions.DependencyInjection;
using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Primitives;

namespace Passport.Core.Application.Commands;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPassportCommands(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<BeginRegistrationCommand, BeginRegistrationResult>, BeginRegistrationCommandHandler>();
        services.AddScoped<ICommandHandler<CompleteRegistrationCommand, Unit>, CompleteRegistrationCommandHandler>();
        services.AddScoped<ICommandHandler<BeginAuthenticationCommand, BeginAuthenticationResult>, BeginAuthenticationCommandHandler>();
        services.AddScoped<ICommandHandler<CompleteAuthenticationCommand, Unit>, CompleteAuthenticationCommandHandler>();
        services.AddScoped<ICommandHandler<BeginRecoveryCommand, Unit>, BeginRecoveryCommandHandler>();
        services.AddScoped<ICommandHandler<VerifyRecoveryCodeCommand, VerifyRecoveryCodeResult>, VerifyRecoveryCodeCommandHandler>();
        services.AddScoped<ICommandHandler<BeginRecoveryRegistrationCommand, BeginRecoveryRegistrationResult>, BeginRecoveryRegistrationCommandHandler>();
        services.AddScoped<ICommandHandler<CompleteRecoveryCommand, Unit>, CompleteRecoveryCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveCredentialCommand, Unit>, RemoveCredentialCommandHandler>();
        services.AddScoped<ICommandHandler<VerifyEmailCommand, Unit>, VerifyEmailCommandHandler>();

        return services;
    }
}