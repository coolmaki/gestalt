using NSubstitute;
using Xunit;
using Gestalt.Lib.Primitives;
using Passport.Core.Application.Commands;
using Passport.Core.Application.Services;

namespace Passport.Core.Application.Tests.Commands.Recovery;

public class BeginRecoveryRegistrationCommandHandlerTests
{
    private readonly IChallengeStore _challengeStore = Substitute.For<IChallengeStore>();
    private readonly IFido2 _fido2 = Substitute.For<IFido2>();
    private readonly BeginRecoveryRegistrationCommandHandler _handler;

    public BeginRecoveryRegistrationCommandHandlerTests()
    {
        _fido2.CreateRegistrationOptionsAsync(Arg.Any<Domain.ValueObjects.Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<(string, string)>.Success(("{}", "internal-state"))));
        _handler = new BeginRecoveryRegistrationCommandHandler(_challengeStore, _fido2);
    }

    [Fact]
    public async Task HandleAsync_ValidToken_ReturnsOptions()
    {
        _challengeStore.GetAndRemoveAsync("recovery:valid-token", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<byte[]>.Some(
                System.Text.Encoding.UTF8.GetBytes("test@example.com"))));

        var result = await _handler.HandleAsync(
            new BeginRecoveryRegistrationCommand("valid-token"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("{}", result.Value.OptionsJson);
    }

    [Fact]
    public async Task HandleAsync_InvalidToken_ReturnsValidationError()
    {
        _challengeStore.GetAndRemoveAsync("recovery:bad-token", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<byte[]>.None));

        var result = await _handler.HandleAsync(
            new BeginRecoveryRegistrationCommand("bad-token"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("recovery_token.invalid", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_InvalidEmailInToken_ReturnsValidationError()
    {
        _challengeStore.GetAndRemoveAsync("recovery:bad-email-token", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<byte[]>.Some(
                System.Text.Encoding.UTF8.GetBytes("not-an-email"))));

        var result = await _handler.HandleAsync(
            new BeginRecoveryRegistrationCommand("bad-email-token"), CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}