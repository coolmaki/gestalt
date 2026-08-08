using NSubstitute;
using Xunit;
using Gestalt.Lib.Primitives;
using Passport.Core.Application.Commands;
using Passport.Core.Application.Services;

namespace Passport.Core.Application.Tests.Commands.Credentials;

/// <summary>
/// Tests for <see cref="BeginAddPasskeyRegistrationCommandHandler"/>.
/// Phase B (WebAuthn credential registration) — step 3 of 4 in the add-passkey flow.
/// </summary>
public class BeginAddPasskeyRegistrationCommandHandlerTests
{
    private readonly IChallengeStore _challengeStore = Substitute.For<IChallengeStore>();
    private readonly IFido2 _fido2 = Substitute.For<IFido2>();
    private readonly BeginAddPasskeyRegistrationCommandHandler _handler;

    public BeginAddPasskeyRegistrationCommandHandlerTests()
    {
        _fido2.CreateRegistrationOptionsAsync(Arg.Any<Domain.ValueObjects.Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<(string, string)>.Success(("{}", "internal-state"))));
        _handler = new BeginAddPasskeyRegistrationCommandHandler(_challengeStore, _fido2);
    }

    [Fact]
    public async Task HandleAsync_ValidToken_ReturnsOptions()
    {
        _challengeStore.GetAndRemoveAsync("add-passkey:valid-token", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<byte[]>.Some(
                System.Text.Encoding.UTF8.GetBytes("test@example.com"))));

        var result = await _handler.HandleAsync(
            new BeginAddPasskeyRegistrationCommand("valid-token"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("{}", result.Value.OptionsJson);
    }

    [Fact]
    public async Task HandleAsync_InvalidToken_ReturnsValidationError()
    {
        _challengeStore.GetAndRemoveAsync("add-passkey:bad-token", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<byte[]>.None));

        var result = await _handler.HandleAsync(
            new BeginAddPasskeyRegistrationCommand("bad-token"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("add_passkey_token.invalid", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_InvalidEmailInToken_ReturnsValidationError()
    {
        _challengeStore.GetAndRemoveAsync("add-passkey:bad-email-token", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<byte[]>.Some(
                System.Text.Encoding.UTF8.GetBytes("not-an-email"))));

        var result = await _handler.HandleAsync(
            new BeginAddPasskeyRegistrationCommand("bad-email-token"), CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}