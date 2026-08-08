using Gestalt.Lib.Application.Commands;
using Gestalt.Lib.Application.Providers;
using Gestalt.Lib.Primitives;
using Passport.Core.Application.Repositories;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands;

internal sealed class RemoveCredentialCommandHandler(
    IUserCommandRepository userRepo,
    IDateTimeProvider clock
) : ICommandHandler<RemoveCredentialCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(RemoveCredentialCommand command, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return emailResult.Error;
        }

        var userOption = await userRepo.FindByEmailAsync(emailResult.Value, cancellationToken);
        if (userOption.IsNone)
        {
            return Error.NotFound("user.not_found", "User not found.");
        }

        User user = userOption.Value;

        if (user.Passkeys.Count <= 1)
        {
            return Error.Conflict("passkey.last_credential", "Cannot remove the last passkey. Use account recovery to replace it.");
        }

        var result = user.RemovePasskey(command.CredentialId, clock.UtcNow());
        if (result.IsFailure)
        {
            return result;
        }

        await userRepo.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}