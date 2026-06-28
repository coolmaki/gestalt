using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;
using Passport.Infrastructure.Auth;
using Passport.Infrastructure.Challenge;
using Passport.Infrastructure.Data;
using Passport.Infrastructure.Data.Repositories;
using Passport.Infrastructure.EmailSender;

namespace Passport.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPassportInfrastructure(this IServiceCollection services, Action<DbContextOptionsBuilder> dbOptions)
    {
        // Database
        services.AddDbContext<PassportDbContext>(dbOptions);

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