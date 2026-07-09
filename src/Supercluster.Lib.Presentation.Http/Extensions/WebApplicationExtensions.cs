using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Supercluster.Lib.Presentation.Http;

namespace Supercluster.Lib.Presentation.Http.Extensions;

public static class WebApplicationExtensions
{
    public static IApplicationBuilder MapEndpoints(this WebApplication app, RouteGroupBuilder? routeGroupBuilder = null)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        IEndpointRouteBuilder builder = routeGroupBuilder is null
            ? app
            : routeGroupBuilder;

        var versionGroups = endpoints.GroupBy(e => e.Version);

        foreach (var group in versionGroups)
        {
            if (group.Key == EndpointVersion.None)
            {
                foreach (var endpoint in group)
                {
                    endpoint.MapEndpoint(builder);
                }
            }
            else
            {
                string versionPrefix = $"/api/{group.Key.ToPathSegment()}";
                var versionGroup = builder.MapGroup(versionPrefix);

                foreach (var endpoint in group)
                {
                    endpoint.MapEndpoint(versionGroup);
                }
            }
        }

        return app;
    }
}