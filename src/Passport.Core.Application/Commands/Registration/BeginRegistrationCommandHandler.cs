using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Ports;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands.Registration;

internal sealed class BeginRegistrationCommandHandler : ICommandHandler<BeginRegistrationCommand, string>
{
    private readonly IUserQueryRepository _userQueryRepo;
    private readonly IFido2 _fido2;
    private readonly IChallengeStore _challengeStore;

    public BeginRegistrationCommandHandler(IUserQueryRepository userQueryRepo, IFido2 fido2, IChallengeStore challengeStore)
    {
        _userQueryRepo = userQueryRepo;
        _fido2 = fido2;
        _challengeStore = challengeStore;
    }

    public async Task<Result<string>> HandleAsync(BeginRegistrationCommand command, CancellationToken ct)
    {
        // 1. Validate email
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return emailResult.Error;
        }

        Email email = emailResult.Value;

        // 2. Check uniqueness
        var existing = await _userQueryRepo.FindByEmailAsync(email.Value, ct);
        if (existing.IsSome)
        {
            return Error.Conflict("email.already_registered", "A user with this email already exists.");
        }

        // 3. Generate WebAuthn registration options
        var optionsResult = await _fido2.CreateRegistrationOptionsAsync(email, ct);
        if (optionsResult.IsFailure)
        {
            return optionsResult.Error;
        }

        (string optionsJson, byte[] challenge) = optionsResult.Value;

        // 4. Store challenge for the completion step
        await _challengeStore.SetAsync(email.Value, challenge, TimeSpan.FromMinutes(5), ct);

        return optionsJson;
    }
}