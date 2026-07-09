using NSubstitute;
using Xunit;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Commands;
using Passport.Core.Application.Repositories;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Tests.Commands;

public class RevokeRefreshTokenCommandHandlerTests
{
    private readonly IUserCommandRepository _userRepo = Substitute.For<IUserCommandRepository>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly RevokeRefreshTokenCommandHandler _handler;

    public RevokeRefreshTokenCommandHandlerTests()
    {
        _clock.UtcNow().Returns(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        _handler = new RevokeRefreshTokenCommandHandler(_userRepo, _clock);
    }

    [Fact]
    public async Task HandleAsync_ValidToken_RevokesIt()
    {
        var user = User.Register(Email.Create("test@example.com").Value, _clock.UtcNow()).Value;
        user.IssueRefreshToken("hash1", _clock.UtcNow(), TimeSpan.FromDays(30));
        _userRepo.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.Some(user)));

        var result = await _handler.HandleAsync(
            new RevokeRefreshTokenCommand("test@example.com", "hash1"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(user.RefreshTokens.First().IsRevoked);
        await _userRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsNotFound()
    {
        _userRepo.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.None));

        var result = await _handler.HandleAsync(
            new RevokeRefreshTokenCommand("test@example.com", "hash1"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("user.not_found", result.Error.Code);
    }
}