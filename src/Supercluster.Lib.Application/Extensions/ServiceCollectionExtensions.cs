using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Application.Queries;

namespace Supercluster.Lib.Application.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="ISender"/> and its implementation.
    /// </summary>
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        services.AddScoped<ISender, Sender>();

        return services;
    }

    /// <summary>
    /// Scans the given assemblies for <see cref="ICommandHandler{TCommand,TResult}"/>
    /// and <see cref="IQueryHandler{TQuery,TResult}"/> implementations and registers them
    /// as scoped services.
    /// </summary>
    public static IServiceCollection AddHandlers(this IServiceCollection services, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            RegisterHandlers(services, assembly, typeof(ICommandHandler<,>));
            RegisterHandlers(services, assembly, typeof(IQueryHandler<,>));
        }

        return services;
    }

    // ------------------------------------------------------------
    // Internal
    // ------------------------------------------------------------

    private static void RegisterHandlers(IServiceCollection services, Assembly assembly, Type openHandlerType)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Select(t => new
            {
                Implementation = t,
                Interface = t.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == openHandlerType),
            })
            .Where(x => x.Interface is not null);

        foreach (var handler in handlerTypes)
        {
            services.AddScoped(handler.Interface!, handler.Implementation);
        }
    }
}