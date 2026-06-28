using Microsoft.AspNetCore.Routing;

namespace Supercluster.Lib.Presentation.Http;

public interface IEndpoint
{
    EndpointVersion Version => EndpointVersion.V1;

    void MapEndpoint(IEndpointRouteBuilder app);
}