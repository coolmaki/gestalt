using Gestalt.Lib.Application.Mediator;
using Gestalt.Lib.Presentation.Http;
using Microsoft.AspNetCore.Mvc;
using Gestalt.Lib.Presentation.Http.Extensions;
using Passport.Core.Application.Commands;

namespace Passport.Presentation.Http.Endpoints;

internal sealed class CompleteAuthenticationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login/complete", async ([FromBody] CompleteAuthenticationCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(command, cancellationToken);
            return result.ToHttpResponse();
        });
    }
}