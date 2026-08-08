using Gestalt.Lib.Application.Mediator;
using Gestalt.Lib.Presentation.Http;
using Microsoft.AspNetCore.Mvc;
using Gestalt.Lib.Presentation.Http.Extensions;
using Passport.Core.Application.Commands;

namespace Passport.Presentation.Http.Endpoints;

/// <summary>
/// POST /api/v1/auth/credentials/add/begin-registration
/// Returns WebAuthn registration options for the add-passkey flow. The client
/// calls <c>navigator.credentials.create()</c> with the returned options, then
/// calls <see cref="CompleteAddPasskeyEndpoint"/> with the attestation.
/// <para>
/// Phase B (WebAuthn credential registration) — step 3 of 4.
/// </para>
/// </summary>
internal sealed class BeginAddPasskeyRegistrationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/credentials/add/begin-registration", async ([FromBody] BeginAddPasskeyRegistrationCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(command, cancellationToken);
            return result.ToHttpResponse();
        });
    }
}