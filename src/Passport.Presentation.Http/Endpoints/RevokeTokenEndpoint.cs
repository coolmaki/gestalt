using Gestalt.Lib.Application.Mediator;
using Gestalt.Lib.Presentation.Http;
using Gestalt.Lib.Presentation.Http.Extensions;
using Passport.Core.Application.Commands;
using System.Security.Claims;

namespace Passport.Presentation.Http.Endpoints;

internal sealed class RevokeTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/auth/tokens/{tokenHash}", async (string tokenHash, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken) =>
        {
            var email = user.FindFirstValue(ClaimTypes.Email)
                         ?? user.FindFirstValue("email");
            if (string.IsNullOrWhiteSpace(email))
            {
                return Results.Unauthorized();
            }

            var command = new RevokeRefreshTokenCommand(email, tokenHash);
            var result = await sender.SendAsync(command, cancellationToken);
            return result.ToHttpResponse();
        });
    }
}