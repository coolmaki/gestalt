using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Ports.Repositories;
using Passport.Core.Application.Ports.Services;

namespace Passport.Core.Application.Commands;

internal sealed class BeginRegistrationCommandHandler(
    IUserQueryRepository userQueryRepo,
    IFido2 fido2,
    IChallengeStore challengeStore
) : ICommandHandler<BeginRegistrationCommand, BeginRegistrationResult>
{
    public async Task<Result<BeginRegistrationResult>> HandleAsync(BeginRegistrationCommand command, CancellationToken ct)
    {
        var emailResult = Domain.ValueObjects.Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return emailResult.Error;
        }

        var email = emailResult.Value;

        var existing = await userQueryRepo.FindByEmailAsync(email.Value, ct);
        if (existing.IsSome)
        {
            return Error.Conflict("email.already_registered", "A user with this email already exists.");
        }

        var optionsResult = await fido2.CreateRegistrationOptionsAsync(email, ct);
        if (optionsResult.IsFailure)
        {
            return optionsResult.Error;
        }

        (string optionsJson, byte[] challenge) = optionsResult.Value;

        await challengeStore.SetAsync(email.Value, challenge, TimeSpan.FromMinutes(5), ct);

        return new BeginRegistrationResult(optionsJson);
    }
}