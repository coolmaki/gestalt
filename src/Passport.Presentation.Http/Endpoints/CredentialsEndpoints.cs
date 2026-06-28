using Supercluster.Lib.Application.Mediator;
using Supercluster.Lib.Presentation.Http;
using Passport.Core.Application.Commands;
using Passport.Core.Application.Queries;

namespace Passport.Presentation.Http.Endpoints;

internal sealed class CredentialsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth/credentials");

        group.MapGet("/", async (string email, ISender sender, CancellationToken cancellationToken) =>
        {
            var query = new GetCredentialsQuery(email);
            var result = await sender.SendAsync(query, cancellationToken);
            return ApiResponse.FromResult(result);
        });

        group.MapDelete("/", async (RemoveCredentialCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(command, cancellationToken);
            return ApiResponse.FromResult(result);
        });
    }
}