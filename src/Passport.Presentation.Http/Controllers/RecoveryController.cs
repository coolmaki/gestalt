using Supercluster.Lib.Application;
using Supercluster.Lib.Application.Mediator;
using Passport.Core.Application.Commands;

namespace Passport.Presentation.Http.Controllers;

internal static class RecoveryController
{
    public static void MapRecoveryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth/recovery");

        group.MapPost("/begin", async (BeginRecoveryCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(command, cancellationToken);
            return ApiResponse.FromResult(result);
        });

        group.MapPost("/verify-code", async (VerifyRecoveryCodeCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(command, cancellationToken);
            return ApiResponse.FromResult(result);
        });

        group.MapPost("/begin-registration", async (BeginRecoveryRegistrationCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(command, cancellationToken);
            return ApiResponse.FromResult(result);
        });

        group.MapPost("/complete", async (CompleteRecoveryCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(command, cancellationToken);
            return ApiResponse.FromResult(result);
        });
    }
}