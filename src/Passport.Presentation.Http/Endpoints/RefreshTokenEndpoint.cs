using Supercluster.Lib.Application.Mediator;
using Supercluster.Lib.Presentation.Http;
using Microsoft.AspNetCore.Mvc;
using Supercluster.Lib.Presentation.Http.Extensions;
using Passport.Core.Application.Commands;

namespace Passport.Presentation.Http.Endpoints;

internal sealed class RefreshTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/token/refresh", async ([FromBody] RefreshAccessTokenRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new RefreshAccessTokenCommand(request.RefreshToken);
            var result = await sender.SendAsync(command, cancellationToken);
            return result.ToHttpResponse();
        });
    }
}