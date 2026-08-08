using Gestalt.Lib.Application.Mediator;
using Gestalt.Lib.Presentation.Http;
using Microsoft.AspNetCore.Mvc;
using Gestalt.Lib.Presentation.Http.Extensions;
using Passport.Core.Application.Queries;

namespace Passport.Presentation.Http.Endpoints;

internal sealed class GetCredentialsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/auth/credentials", async (string email, ISender sender, CancellationToken cancellationToken) =>
        {
            var query = new GetCredentialsQuery(email);
            var result = await sender.SendAsync(query, cancellationToken);
            return result.ToHttpResponse();
        });
    }
}