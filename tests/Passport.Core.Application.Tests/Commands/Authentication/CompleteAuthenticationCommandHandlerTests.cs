using NSubstitute;
using Xunit;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Commands;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Tests.Commands.Authentication;

public class CompleteAuthenticationCommandHandlerTests
{
    private readonly IUserCommandRepository _userRepo = Substitute.For<IUserCommandRepository>();
    private readonly IChallengeStore _challengeStore = Substitute.For<IChallengeStore>();
    private readonly IFido2 _fido2 = Substitute.For<IFido2>();
    private readonly CompleteAuthenticationCommandHandler _handler;

    public CompleteAuthenticationCommandHandlerTests()
    {
        _handler = new CompleteAuthenticationCommandHandler(_userRepo, _challengeStore, _fido2);
    }

    [Fact]
    public async Task HandleAsync_ValidAssertion_ReturnsSuccess()
    {
        var command = new CompleteAuthenticationCommand("test@example.com", "fake-assertion");
        var user = CreateUserWithPasskey();
        _userRepo.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.Some(user)));
        _challengeStore.GetAndRemoveAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<byte[]>.Some([1])));
        _fido2.CompleteAssertionAsync(Arg.Any<byte[]>(), "fake-assertion", Arg.Any<byte[]>(), Arg.Any<uint>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<uint>.Success(1)));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _userRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsNotFound()
    {
        var command = new CompleteAuthenticationCommand("test@example.com", "fake");
        _userRepo.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.None));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("user.not_found", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_ChallengeExpired_ReturnsValidationError()
    {
        var command = new CompleteAuthenticationCommand("test@example.com", "fake");
        var user = CreateUserWithPasskey();
        _userRepo.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.Some(user)));
        _challengeStore.GetAndRemoveAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<byte[]>.None));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("challenge.expired", result.Error.Code);
    }

    private static User CreateUserWithPasskey()
    {
        var user = User.Register(Email.Create("test@example.com").Value, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)).Value;
        user.AddPasskey([1], [2], 0, new DateTimeOffset(2026, 1, 1, 0, 0, 1, TimeSpan.Zero));
        return user;
    }
}