using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;
using Passport.Infrastructure.Configuration;
using Passport.Infrastructure.Extensions;
using Passport.Infrastructure.Persistence;
using Passport.Infrastructure.Repositories;
using Passport.Infrastructure.Services;
using Gestalt.Lib.Infrastructure.Email;

namespace Passport.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPassportInfrastructure(
        this IServiceCollection services,
        PersistenceConfiguration persistenceConfig,
        Passport.Core.Application.Configuration.SigningKeyConfiguration signingKeyConfig)
    {
        // Configuration
        services.AddSingleton(persistenceConfig);

        // Signing Key
        var keyPath = signingKeyConfig.KeyPath;
        var ecdsaKey = LoadOrCreateEcdsaKey(keyPath);
        var keyId = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(8));
        var securityKey = new ECDsaSecurityKey(ecdsaKey) { KeyId = keyId };

        services.AddSingleton(ecdsaKey);
        services.AddSingleton(securityKey);

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
        services.AddScoped<IRefreshTokenQueryRepository, RefreshTokenQueryRepository>();

        // Dapper requires DbConnection — resolve from EF Core DbContext
        services.AddScoped(provider =>
        {
            var dbContext = provider.GetRequiredService<PassportDbContext>();
            return dbContext.Database.GetDbConnection();
        });

        // Services
        services.AddScoped<IFido2, Fido2Service>();
        services.AddSingleton<IChallengeStore, MemoryChallengeStore>();
        services.AddScoped<ICodeDeliveryService, CodeDeliveryService>();
        services.AddScoped<IEmailSender, LoggingEmailSender>();
        services.AddSingleton<ITokenService, TokenService>();

        return services;
    }

    private static ECDsa LoadOrCreateEcdsaKey(string keyPath)
    {
        if (File.Exists(keyPath))
        {
            var pem = File.ReadAllText(keyPath);
            var ecdsaKey = ECDsa.Create();
            ecdsaKey.ImportFromPem(pem);
            return ecdsaKey;
        }

        var newKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var newPem = newKey.ExportPkcs8PrivateKeyPem();
        File.WriteAllText(keyPath, newPem);
        return newKey;
    }
}