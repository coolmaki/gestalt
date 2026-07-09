using NSubstitute;
using Xunit;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Commands;
using Passport.Core.Application.Repositories;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Tests.Commands.Credentials;

public class RemoveCredentialCommandHandlerTests
{
    private readonly IUserCommandRepository _userRepo = Substitute.For<IUserCommandRepository>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly RemoveCredentialCommandHandler _handler;

    public RemoveCredentialCommandHandlerTests()
    {
        _clock.UtcNow().Returns(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        _handler = new RemoveCredentialCommandHandler(_userRepo, _clock);
    }

    [Fact]
    public async Task HandleAsync_TwoCredentials_RemovesSuccessfully()
    {
        var user = User.Register(Email.Create("test@example.com").Value, _clock.UtcNow()).Value;
        user.AddPasskey([1], [1], 0, _clock.UtcNow());
        user.AddPasskey([2], [2], 0, _clock.UtcNow());

        _userRepo.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.Some(user)));

        var result = await _handler.HandleAsync(new RemoveCredentialCommand("test@example.com", [1]), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(user.Passkeys);
    }

    [Fact]
    public async Task HandleAsync_LastCredential_ReturnsConflict()
    {
        var user = User.Register(Email.Create("test@example.com").Value, _clock.UtcNow()).Value;
        user.AddPasskey([1], [1], 0, _clock.UtcNow());

        _userRepo.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.Some(user)));

        var result = await _handler.HandleAsync(new RemoveCredentialCommand("test@example.com", [1]), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("passkey.last_credential", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsNotFound()
    {
        _userRepo.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.None));

        var result = await _handler.HandleAsync(new RemoveCredentialCommand("test@example.com", [1]), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("user.not_found", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_CredentialNotFound_ReturnsNotFound()
    {
        var user = User.Register(Email.Create("test@example.com").Value, _clock.UtcNow()).Value;
        user.AddPasskey([1], [1], 0, _clock.UtcNow());
        user.AddPasskey([2], [2], 0, _clock.UtcNow());

        _userRepo.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.Some(user)));

        var result = await _handler.HandleAsync(new RemoveCredentialCommand("test@example.com", [9, 9]), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("passkey.not_found", result.Error.Code);
    }
}