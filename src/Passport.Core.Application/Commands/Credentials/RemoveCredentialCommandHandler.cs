using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Ports;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands.Credentials;

internal sealed class RemoveCredentialCommandHandler : ICommandHandler<RemoveCredentialCommand, Unit>
{
    private readonly IUserCommandRepository _userRepo;
    private readonly IDateTimeProvider _clock;

    public RemoveCredentialCommandHandler(IUserCommandRepository userRepo, IDateTimeProvider clock)
    {
        _userRepo = userRepo;
        _clock = clock;
    }

    public async Task<Result<Unit>> HandleAsync(RemoveCredentialCommand command, CancellationToken ct)
    {
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return emailResult.Error;
        }

        var userOption = await _userRepo.FindByEmailAsync(emailResult.Value, ct);
        if (userOption.IsNone)
        {
            return Error.NotFound("user.not_found", "User not found.");
        }

        User user = userOption.Value;

        if (user.Passkeys.Count <= 1)
        {
            return Error.Conflict("passkey.last_credential", "Cannot remove the last passkey. Use account recovery to replace it.");
        }

        var result = user.RemovePasskey(command.CredentialId, _clock.UtcNow());
        if (result.IsFailure)
        {
            return result;
        }

        await _userRepo.SaveChangesAsync(ct);

        return Unit.Value;
    }
}