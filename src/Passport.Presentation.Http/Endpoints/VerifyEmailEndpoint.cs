using Supercluster.Lib.Application.Mediator;
using Supercluster.Lib.Presentation.Http;
using Supercluster.Lib.Presentation.Http.Extensions;
using Passport.Core.Application.Commands;

namespace Passport.Presentation.Http.Endpoints;

internal sealed class VerifyEmailEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register/verify-email", async (VerifyEmailCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(command, cancellationToken);
            return result.ToHttpResponse();
        });
    }
}