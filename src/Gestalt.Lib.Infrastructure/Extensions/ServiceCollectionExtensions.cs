using Microsoft.Extensions.DependencyInjection;
using Gestalt.Lib.Application.Providers;
using Gestalt.Lib.Infrastructure.Providers;

namespace Gestalt.Lib.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProviders(this IServiceCollection services)
    {
        return services
            .AddSingleton<IDateTimeProvider, DateTimeProvider>()
            .AddSingleton<IGuidProvider, GuidProvider>();
    }
}
