using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;
using Passport.Infrastructure.Configuration;
using Passport.Infrastructure.Persistence;
using Passport.Infrastructure.Repositories;
using Passport.Infrastructure.Services;

namespace Passport.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPassportInfrastructure(
        this IServiceCollection services,
        PersistenceConfiguration persistenceConfig)
    {
        // Configuration
        services.AddSingleton(persistenceConfig);

        // Database — provider selection
        services.AddDbContext<PassportDbContext>(options =>
        {
            if (persistenceConfig.Provider == PersistenceProvider.Postgres)
            {
                options.UseNpgsql(persistenceConfig.ConnectionString);
            }
            else
            {
                options.UseSqlite(persistenceConfig.ConnectionString);
            }
        });

        // Repositories
        services.AddScoped<IUserCommandRepository, UserCommandRepository>();
        services.AddScoped<IUserQueryRepository, UserQueryRepository>();
        services.AddScoped<IRecoveryCodeRepository, RecoveryCodeRepository>();

        // Services
        services.AddScoped<IFido2, Fido2Service>();
        services.AddScoped<IChallengeStore, MemoryChallengeStore>();
        services.AddScoped<IEmailSender, LoggingEmailSender>();

        return services;
    }
}