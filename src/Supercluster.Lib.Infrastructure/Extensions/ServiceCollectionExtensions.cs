using Microsoft.Extensions.DependencyInjection;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Infrastructure.Providers;

namespace Supercluster.Lib.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProviders(this IServiceCollection services)
    {
        return services
            .AddSingleton<IDateTimeProvider, DateTimeProvider>()
            .AddSingleton<IGuidProvider, GuidProvider>();
    }
}
