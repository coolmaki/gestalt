using Microsoft.Extensions.DependencyInjection;
using Gestalt.Lib.Application.Extensions;

namespace Passport.Core.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPassportCommandsAndQueries(this IServiceCollection services)
    {
        services.AddHandlers(typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }
}