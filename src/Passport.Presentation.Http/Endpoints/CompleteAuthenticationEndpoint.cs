using Supercluster.Lib.Application.Mediator;
using Supercluster.Lib.Presentation.Http;
using Passport.Core.Application.Commands;

namespace Passport.Presentation.Http.Endpoints;

internal sealed class CompleteAuthenticationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login/complete", async (CompleteAuthenticationCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(command, cancellationToken);
            return ApiResponse.FromResult(result);
        });
    }
}