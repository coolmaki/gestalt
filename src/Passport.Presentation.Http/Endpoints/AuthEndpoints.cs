using Supercluster.Lib.Application.Mediator;
using Supercluster.Lib.Presentation.Http;
using Passport.Core.Application.Commands;

namespace Passport.Presentation.Http.Endpoints;

internal sealed class AuthEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth");

        group.MapPost("/register/begin", async (BeginRegistrationCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(command, cancellationToken);
            return ApiResponse.FromResult(result);
        });

        group.MapPost("/register/complete", async (CompleteRegistrationCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(command, cancellationToken);
            return ApiResponse.FromResult(result);
        });

        group.MapPost("/register/verify-email", async (VerifyEmailCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(command, cancellationToken);
            return ApiResponse.FromResult(result);
        });

        group.MapPost("/login/begin", async (BeginAuthenticationCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(command, cancellationToken);
            return ApiResponse.FromResult(result);
        });

        group.MapPost("/login/complete", async (CompleteAuthenticationCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(command, cancellationToken);
            return ApiResponse.FromResult(result);
        });
    }
}