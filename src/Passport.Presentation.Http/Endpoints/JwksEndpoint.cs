using Microsoft.AspNetCore.Http;
using Gestalt.Lib.Presentation.Http;
using Passport.Core.Application.Services;

namespace Passport.Presentation.Http.Endpoints;

internal sealed class JwksEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.None;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/.well-known/jwks.json", (ITokenService tokenService) =>
        {
            var key = tokenService.GetSigningKey();
            return Results.Json(new { keys = new[] { key } });
        });
    }
}