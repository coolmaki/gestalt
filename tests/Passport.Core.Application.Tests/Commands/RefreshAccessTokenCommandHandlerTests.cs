using System.Security.Cryptography;
using System.Text;
using NSubstitute;
using Xunit;
using Gestalt.Lib.Application.Providers;
using Gestalt.Lib.Primitives;
using Passport.Core.Application.Commands;
using Passport.Core.Application.Configuration;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Tests.Commands;

public class RefreshAccessTokenCommandHandlerTests
{
    private readonly IUserCommandRepository _userRepo = Substitute.For<IUserCommandRepository>();
    private readonly IRefreshTokenQueryRepository _refreshTokenQueryRepo = Substitute.For<IRefreshTokenQueryRepository>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly ApplicationConfiguration _appConfig = new()
    {
        AccessToken = new AccessTokenConfiguration { LifetimeMinutes = 15 },
        RefreshToken = new RefreshTokenConfiguration { LifetimeDays = 30, RotationEnabled = true },
    };
    private readonly RefreshAccessTokenCommandHandler _handler;

    public RefreshAccessTokenCommandHandlerTests()
    {
        _clock.UtcNow().Returns(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        _tokenService.GenerateAccessToken(Arg.Any<string>()).Returns("new-access-token");
        _tokenService.GenerateRefreshToken().Returns(("new-refresh-token", "new-hash"));
        _handler = new RefreshAccessTokenCommandHandler(_userRepo, _refreshTokenQueryRepo, _tokenService, _clock, _appConfig);
    }

    [Fact]
    public async Task HandleAsync_ValidToken_ReturnsNewPair()
    {
        var rawToken = "test-refresh-token";
        var tokenHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

        var user = User.Register(Email.Create("test@example.com").Value, _clock.UtcNow()).Value;
        user.IssueRefreshToken(tokenHash, _clock.UtcNow(), TimeSpan.FromDays(30));
        _refreshTokenQueryRepo.FindEmailByHashAsync(tokenHash, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<string>.Some("test@example.com")));
        _userRepo.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.Some(user)));

        var result = await _handler.HandleAsync(
            new RefreshAccessTokenCommand(rawToken), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("new-access-token", result.Value.AccessToken);
        Assert.Equal("new-refresh-token", result.Value.RefreshToken);
        Assert.True(user.RefreshTokens.First(t => t.TokenHash == tokenHash).IsRevoked);
    }

    [Fact]
    public async Task HandleAsync_TokenNotFound_ReturnsNotFound()
    {
        _refreshTokenQueryRepo.FindEmailByHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<string>.None));

        var result = await _handler.HandleAsync(
            new RefreshAccessTokenCommand("unknown-token"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("token.not_found", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_RevokedToken_ReturnsValidationError()
    {
        var rawToken = "test-refresh-token";
        var tokenHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

        var user = User.Register(Email.Create("test@example.com").Value, _clock.UtcNow()).Value;
        var token = user.IssueRefreshToken(tokenHash, _clock.UtcNow(), TimeSpan.FromDays(30));
        token.Revoke(_clock.UtcNow());

        _refreshTokenQueryRepo.FindEmailByHashAsync(tokenHash, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<string>.Some("test@example.com")));
        _userRepo.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.Some(user)));

        var result = await _handler.HandleAsync(
            new RefreshAccessTokenCommand(rawToken), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("token.revoked", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_ExpiredToken_ReturnsValidationError()
    {
        var rawToken = "test-refresh-token";
        var tokenHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

        var expired = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);
        var expiredClock = Substitute.For<IDateTimeProvider>();
        expiredClock.UtcNow().Returns(expired);

        var user = User.Register(Email.Create("test@example.com").Value, expired).Value;
        user.IssueRefreshToken(tokenHash, expired - TimeSpan.FromDays(31), TimeSpan.FromDays(30));

        _refreshTokenQueryRepo.FindEmailByHashAsync(tokenHash, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<string>.Some("test@example.com")));
        _userRepo.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.Some(user)));

        var expiredHandler = new RefreshAccessTokenCommandHandler(_userRepo, _refreshTokenQueryRepo, _tokenService, expiredClock, _appConfig);

        var result = await expiredHandler.HandleAsync(
            new RefreshAccessTokenCommand(rawToken), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("token.expired", result.Error.Code);
    }
}