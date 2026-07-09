using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Configuration;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands;

internal sealed class CreateSessionCommandHandler(
    IUserCommandRepository userRepo,
    ITokenService tokenService,
    IDateTimeProvider clock,
    ApplicationConfiguration appConfig
) : ICommandHandler<CreateSessionCommand, SessionResult>
{
    public async Task<Result<SessionResult>> HandleAsync(CreateSessionCommand command, CancellationToken cancellationToken)
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
        var now = clock.UtcNow();

        var accessToken = tokenService.GenerateAccessToken(command.Email);
        var (rawToken, tokenHash) = tokenService.GenerateRefreshToken();

        var refreshTtl = TimeSpan.FromDays(appConfig.RefreshToken.LifetimeDays);

        user.IssueRefreshToken(tokenHash, now, refreshTtl);

        await userRepo.SaveChangesAsync(cancellationToken);

        var result = new SessionResult(
            accessToken,
            rawToken,
            appConfig.AccessToken.LifetimeMinutes * 60
        );

        return result;
    }
}