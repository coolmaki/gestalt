using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Supercluster.Lib.Presentation.Http.Extensions;

public static class WebApplicationExtensions
{
    public static IApplicationBuilder MapEndpoints(this WebApplication app, RouteGroupBuilder? routeGroupBuilder = null)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        IEndpointRouteBuilder builder = routeGroupBuilder is null
            ? app
            : routeGroupBuilder;

        // Group by version so all v1 endpoints share a /api/v1 prefix
        var versionGroups = endpoints.GroupBy(e => e.Version);

        foreach (var group in versionGroups)
        {
            string versionPrefix = $"/api/{group.Key}";
            var versionGroup = builder.MapGroup(versionPrefix);

            foreach (var endpoint in group)
            {
                endpoint.MapEndpoint(versionGroup);
            }
        }

        return app;
    }
}