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
    [Fact]
    public async Task HandleAsync_TwoCredentials_RemovesSuccessfully()
    {
        var userRepo = Substitute.For<IUserCommandRepository>();
        var clock = Substitute.For<IDateTimeProvider>();
        clock.UtcNow().Returns(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

        var user = User.Register(Email.Create("test@example.com").Value, clock.UtcNow()).Value;
        user.AddPasskey([1], [1], 0, clock.UtcNow());
        user.AddPasskey([2], [2], 0, clock.UtcNow());

        userRepo.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.Some(user)));

        var handler = new RemoveCredentialCommandHandler(userRepo, clock);

        var result = await handler.HandleAsync(new RemoveCredentialCommand("test@example.com", [1]), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(user.Passkeys);
    }

    [Fact]
    public async Task HandleAsync_LastCredential_ReturnsConflict()
    {
        var userRepo = Substitute.For<IUserCommandRepository>();
        var clock = Substitute.For<IDateTimeProvider>();
        clock.UtcNow().Returns(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

        var user = User.Register(Email.Create("test@example.com").Value, clock.UtcNow()).Value;
        user.AddPasskey([1], [1], 0, clock.UtcNow());

        userRepo.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.Some(user)));

        var handler = new RemoveCredentialCommandHandler(userRepo, clock);

        var result = await handler.HandleAsync(new RemoveCredentialCommand("test@example.com", [1]), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("passkey.last_credential", result.Error.Code);
    }
}