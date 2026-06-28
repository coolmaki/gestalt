using Supercluster.Lib.Application.Mediator;
using Supercluster.Lib.Presentation.Http;
using Passport.Core.Application.Commands;

namespace Passport.Presentation.Http.Endpoints;

internal sealed class BeginAuthenticationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login/begin", async (BeginAuthenticationCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(command, cancellationToken);
            return ApiResponse.FromResult(result);
        });
    }
}