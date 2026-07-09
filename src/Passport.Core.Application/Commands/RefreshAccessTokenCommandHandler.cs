using System.Security.Cryptography;
using System.Text;
using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Configuration;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands;

internal sealed class RefreshAccessTokenCommandHandler(
    IUserCommandRepository userRepo,
    IRefreshTokenQueryRepository refreshTokenQueryRepo,
    ITokenService tokenService,
    IDateTimeProvider clock,
    ApplicationConfiguration appConfig
) : ICommandHandler<RefreshAccessTokenCommand, SessionResult>
{
    public async Task<Result<SessionResult>> HandleAsync(RefreshAccessTokenCommand command, CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(command.RefreshToken);
        var now = clock.UtcNow();

        var emailOption = await refreshTokenQueryRepo.FindEmailByHashAsync(tokenHash, cancellationToken);
        if (emailOption.IsNone)
        {
            return Error.NotFound("token.not_found", "Refresh token not found.");
        }

        var emailResult = Email.Create(emailOption.Value);
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

        var token = user.RefreshTokens.FirstOrDefault(t => t.TokenHash == tokenHash);
        if (token is null)
        {
            return Error.NotFound("token.not_found", "Refresh token not found.");
        }

        if (token.IsRevoked)
        {
            return Error.Validation("token.revoked", "Refresh token has been revoked.");
        }

        if (token.IsExpired(now))
        {
            return Error.Validation("token.expired", "Refresh token has expired.");
        }

        if (appConfig.RefreshToken.RotationEnabled)
        {
            token.Revoke(now);
        }

        var accessToken = tokenService.GenerateAccessToken(
            user.Email.Value
        );

        var (rawToken, newTokenHash) = tokenService.GenerateRefreshToken();
        var refreshTtl = TimeSpan.FromDays(appConfig.RefreshToken.LifetimeDays);

        user.IssueRefreshToken(newTokenHash, now, refreshTtl);

        await userRepo.SaveChangesAsync(cancellationToken);

        var result = new SessionResult(
            accessToken,
            rawToken,
            appConfig.AccessToken.LifetimeMinutes * 60
        );

        return result;
    }

    private static string HashToken(string token)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexStringLower(hash);
    }
}