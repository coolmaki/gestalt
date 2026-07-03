using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;
using Passport.Infrastructure.Configuration;
using Passport.Infrastructure.Extensions;
using Passport.Infrastructure.Persistence;
using Passport.Infrastructure.Repositories;
using Passport.Infrastructure.Services;

namespace Passport.Infrastructure.Extensions;

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
            persistenceConfig.Provider.Configure(
                configureSqlite: () => options.UseSqlite(persistenceConfig.ConnectionString),
                configurePostgres: () => options.UseNpgsql(persistenceConfig.ConnectionString));
        });

        // Repositories
        services.AddScoped<IUserCommandRepository, UserCommandRepository>();
        services.AddScoped<IUserQueryRepository, UserQueryRepository>();
        services.AddScoped<IRecoveryCodeRepository, RecoveryCodeRepository>();

        // Dapper requires DbConnection — resolve from EF Core DbContext
        services.AddScoped(provider =>
        {
            var dbContext = provider.GetRequiredService<PassportDbContext>();
            return dbContext.Database.GetDbConnection();
        });

        // Services
        services.AddScoped<IFido2, Fido2Service>();
        services.AddSingleton<IChallengeStore, MemoryChallengeStore>();
        services.AddScoped<IEmailSender, LoggingEmailSender>();

        return services;
    }
}