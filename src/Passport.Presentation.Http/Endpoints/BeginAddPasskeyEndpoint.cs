using Supercluster.Lib.Application.Mediator;
using Supercluster.Lib.Presentation.Http;
using Microsoft.AspNetCore.Mvc;
using Supercluster.Lib.Presentation.Http.Extensions;
using Passport.Core.Application.Commands;

namespace Passport.Presentation.Http.Endpoints;

/// <summary>
/// POST /api/v1/auth/credentials/add/begin
/// Begins the add-passkey-from-new-device flow. Sends a 6-digit verification code
/// to the user's verified email. Silently succeeds if the email is not found or
/// not verified (no user enumeration).
/// <para>
/// Phase A (email verification) — step 1 of 4.
/// </para>
/// </summary>
internal sealed class BeginAddPasskeyEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/credentials/add/begin", async ([FromBody] BeginAddPasskeyCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(command, cancellationToken);
            return result.ToHttpResponse();
        });
    }
}