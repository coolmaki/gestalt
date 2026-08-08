using Gestalt.Lib.Application.Mediator;
using Gestalt.Lib.Presentation.Http;
using Microsoft.AspNetCore.Mvc;
using Gestalt.Lib.Presentation.Http.Extensions;
using Passport.Core.Application.Commands;

namespace Passport.Presentation.Http.Endpoints;

/// <summary>
/// POST /api/v1/auth/credentials/add/verify
/// Validates the device verification code and returns an <c>AddPasskeyToken</c>.
/// The token authorizes WebAuthn credential registration in the subsequent steps.
/// <para>
/// Phase A (email verification) — step 2 of 4.
/// </para>
/// </summary>
internal sealed class VerifyAddPasskeyCodeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/credentials/add/verify", async ([FromBody] VerifyAddPasskeyCodeCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(command, cancellationToken);
            return result.ToHttpResponse();
        });
    }
}