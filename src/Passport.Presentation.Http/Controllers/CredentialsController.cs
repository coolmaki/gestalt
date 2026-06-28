using Supercluster.Lib.Application;
using Supercluster.Lib.Application.Mediator;
using Passport.Core.Application.Commands;
using Passport.Core.Application.Queries;

namespace Passport.Presentation.Http.Controllers;

internal static class CredentialsController
{
    public static void MapCredentialsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth/credentials");

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