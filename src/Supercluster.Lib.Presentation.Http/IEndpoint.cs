using Microsoft.AspNetCore.Routing;

namespace Supercluster.Lib.Presentation.Http;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
