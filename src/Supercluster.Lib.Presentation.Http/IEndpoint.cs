using Microsoft.AspNetCore.Routing;

namespace Supercluster.Lib.Presentation.Http;

public interface IEndpoint
{
    string Version => "v1";

    void MapEndpoint(IEndpointRouteBuilder app);
}