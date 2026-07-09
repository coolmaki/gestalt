using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Repositories;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands;

internal sealed class RevokeAllUserTokensCommandHandler(
    IUserCommandRepository userRepo,
    IDateTimeProvider clock
) : ICommandHandler<RevokeAllUserTokensCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(RevokeAllUserTokensCommand command, CancellationToken cancellationToken)
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

        var user = userOption.Value;

        user.RevokeAllRefreshTokens(clock.UtcNow());

        await userRepo.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}