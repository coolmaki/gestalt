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

public class CreateSessionCommandHandlerTests
{
    private readonly IUserCommandRepository _userRepo = Substitute.For<IUserCommandRepository>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly ApplicationConfiguration _appConfig = new()
    {
        AccessToken = new AccessTokenConfiguration { LifetimeMinutes = 15 },
        RefreshToken = new RefreshTokenConfiguration { LifetimeDays = 30 },
    };
    private readonly CreateSessionCommandHandler _handler;

    public CreateSessionCommandHandlerTests()
    {
        _clock.UtcNow().Returns(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        _tokenService.GenerateAccessToken(Arg.Any<string>()).Returns("access-token");
        _tokenService.GenerateRefreshToken().Returns(("refresh-token", "hash"));
        _handler = new CreateSessionCommandHandler(_userRepo, _tokenService, _clock, _appConfig);
    }

    [Fact]
    public async Task HandleAsync_ValidUser_ReturnsTokenPair()
    {
        var command = new CreateSessionCommand("test@example.com");
        var user = User.Register(Email.Create("test@example.com").Value, _clock.UtcNow()).Value;
        _userRepo.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.Some(user)));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("access-token", result.Value.AccessToken);
        Assert.Equal("refresh-token", result.Value.RefreshToken);
        Assert.Equal(900, result.Value.ExpiresIn);
        await _userRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsNotFound()
    {
        var command = new CreateSessionCommand("test@example.com");
        _userRepo.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.None));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("user.not_found", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_InvalidEmail_ReturnsValidationError()
    {
        var command = new CreateSessionCommand("not-an-email");

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("email.invalid_format", result.Error.Code);
    }
}