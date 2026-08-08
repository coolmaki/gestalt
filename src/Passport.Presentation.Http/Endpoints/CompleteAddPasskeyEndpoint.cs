using Gestalt.Lib.Application.Mediator;
using Gestalt.Lib.Presentation.Http;
using Microsoft.AspNetCore.Mvc;
using Gestalt.Lib.Presentation.Http.Extensions;
using Passport.Core.Application.Commands;

namespace Passport.Presentation.Http.Endpoints;

/// <summary>
/// POST /api/v1/auth/credentials/add/complete
/// Completes the add-passkey-from-new-device flow. Validates the WebAuthn
/// attestation and adds the new passkey credential to the user's account.
/// Unlike account recovery, this does NOT remove existing passkeys — it adds
/// the new one alongside them.
/// <para>
/// Phase B (WebAuthn credential registration) — step 4 of 4.
/// </para>
/// </summary>
internal sealed class CompleteAddPasskeyEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/credentials/add/complete", async ([FromBody] CompleteAddPasskeyCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(command, cancellationToken);
            return result.ToHttpResponse();
        });
    }
}