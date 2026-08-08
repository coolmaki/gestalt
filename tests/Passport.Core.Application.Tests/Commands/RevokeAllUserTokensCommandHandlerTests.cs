using NSubstitute;
using Xunit;
using Gestalt.Lib.Application.Providers;
using Gestalt.Lib.Primitives;
using Passport.Core.Application.Commands;
using Passport.Core.Application.Repositories;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Tests.Commands;

public class RevokeAllUserTokensCommandHandlerTests
{
    private readonly IUserCommandRepository _userRepo = Substitute.For<IUserCommandRepository>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly RevokeAllUserTokensCommandHandler _handler;

    public RevokeAllUserTokensCommandHandlerTests()
    {
        _clock.UtcNow().Returns(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        _handler = new RevokeAllUserTokensCommandHandler(_userRepo, _clock);
    }

    [Fact]
    public async Task HandleAsync_ValidUser_RevokesAll()
    {
        var user = User.Register(Email.Create("test@example.com").Value, _clock.UtcNow()).Value;
        user.IssueRefreshToken("hash1", _clock.UtcNow(), TimeSpan.FromDays(30));
        user.IssueRefreshToken("hash2", _clock.UtcNow(), TimeSpan.FromDays(30));
        _userRepo.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.Some(user)));

        var result = await _handler.HandleAsync(
            new RevokeAllUserTokensCommand("test@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.All(user.RefreshTokens, t => Assert.True(t.IsRevoked));
        await _userRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsNotFound()
    {
        _userRepo.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.None));

        var result = await _handler.HandleAsync(
            new RevokeAllUserTokensCommand("test@example.com"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("user.not_found", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_InvalidEmail_ReturnsValidationError()
    {
        var result = await _handler.HandleAsync(
            new RevokeAllUserTokensCommand("not-an-email"), CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}