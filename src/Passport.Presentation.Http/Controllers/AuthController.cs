using Supercluster.Lib.Application;
using Supercluster.Lib.Application.Mediator;
using Passport.Core.Application.Commands;
using Passport.Core.Application.Queries;

namespace Passport.Presentation.Http.Controllers;

internal static class AuthController
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

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