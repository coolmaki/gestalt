using Microsoft.Extensions.DependencyInjection;
using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Application.Queries;
using Supercluster.Lib.Primitives;

namespace Passport.Core.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPassportCommands(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<Commands.BeginRegistrationCommand, Commands.BeginRegistrationResult>, Commands.BeginRegistrationCommandHandler>();
        services.AddScoped<ICommandHandler<Commands.CompleteRegistrationCommand, Unit>, Commands.CompleteRegistrationCommandHandler>();
        services.AddScoped<ICommandHandler<Commands.BeginAuthenticationCommand, Commands.BeginAuthenticationResult>, Commands.BeginAuthenticationCommandHandler>();
        services.AddScoped<ICommandHandler<Commands.CompleteAuthenticationCommand, Unit>, Commands.CompleteAuthenticationCommandHandler>();
        services.AddScoped<ICommandHandler<Commands.BeginRecoveryCommand, Unit>, Commands.BeginRecoveryCommandHandler>();
        services.AddScoped<ICommandHandler<Commands.VerifyRecoveryCodeCommand, Commands.VerifyRecoveryCodeResult>, Commands.VerifyRecoveryCodeCommandHandler>();
        services.AddScoped<ICommandHandler<Commands.BeginRecoveryRegistrationCommand, Commands.BeginRecoveryRegistrationResult>, Commands.BeginRecoveryRegistrationCommandHandler>();
        services.AddScoped<ICommandHandler<Commands.CompleteRecoveryCommand, Unit>, Commands.CompleteRecoveryCommandHandler>();
        services.AddScoped<ICommandHandler<Commands.RemoveCredentialCommand, Unit>, Commands.RemoveCredentialCommandHandler>();
        services.AddScoped<ICommandHandler<Commands.VerifyEmailCommand, Unit>, Commands.VerifyEmailCommandHandler>();

        return services;
    }

    public static IServiceCollection AddPassportQueries(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<Queries.FindUserQuery, Queries.FindUserResult>, Queries.FindUserQueryHandler>();
        services.AddScoped<IQueryHandler<Queries.GetCredentialsQuery, Queries.GetCredentialsResult>, Queries.GetCredentialsQueryHandler>();

        return services;
    }
}