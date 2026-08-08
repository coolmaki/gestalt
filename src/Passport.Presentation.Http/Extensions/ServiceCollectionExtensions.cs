using Microsoft.Extensions.DependencyInjection;
using Gestalt.Lib.Presentation.Http.Extensions;

namespace Passport.Presentation.Http.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPassportEndpoints(this IServiceCollection services)
    {
        services.AddEndpoints(typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }
}