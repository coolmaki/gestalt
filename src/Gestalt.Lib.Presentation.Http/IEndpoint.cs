using Microsoft.AspNetCore.Routing;

namespace Gestalt.Lib.Presentation.Http;

public interface IEndpoint
{
    EndpointVersion Version => EndpointVersion.V1;

    void MapEndpoint(IEndpointRouteBuilder app);
}