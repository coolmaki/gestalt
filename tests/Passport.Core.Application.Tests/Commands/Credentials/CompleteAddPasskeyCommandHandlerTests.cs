using NSubstitute;
using Xunit;
using Gestalt.Lib.Application.Providers;
using Gestalt.Lib.Primitives;
using Passport.Core.Application.Commands;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Tests.Commands.Credentials;

/// <summary>
/// Tests for <see cref="CompleteAddPasskeyCommandHandler"/>.
/// Phase B (WebAuthn credential registration) — step 4 of 4 in the add-passkey flow.
/// Unlike the recovery flow, this handler does NOT remove existing passkeys — it
/// adds the new one alongside them.
/// </summary>
public class CompleteAddPasskeyCommandHandlerTests
{
    private readonly IUserCommandRepository _userRepo = Substitute.For<IUserCommandRepository>();
    private readonly IChallengeStore _challengeStore = Substitute.For<IChallengeStore>();
    private readonly IFido2 _fido2 = Substitute.For<IFido2>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly CompleteAddPasskeyCommandHandler _handler;

    public CompleteAddPasskeyCommandHandlerTests()
    {
        _clock.UtcNow().Returns(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        _fido2.CompleteRegistrationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<(byte[], byte[], uint)>.Success(([1], [2], 0))));
        _handler = new CompleteAddPasskeyCommandHandler(_userRepo, _challengeStore, _fido2, _clock);
    }

    [Fact]
    public async Task HandleAsync_ValidToken_AddsNewPasskeyPreservingExisting()
    {
        var user = User.Register(Email.Create("test@example.com").Value, _clock.UtcNow()).Value;
        user.AddPasskey([9], [9], 0, _clock.UtcNow());

        _challengeStore.GetAndRemoveAsync("add-passkey:valid-token", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<byte[]>.Some(
                System.Text.Encoding.UTF8.GetBytes("test@example.com"))));
        _userRepo.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.Some(user)));
        _challengeStore.GetAndRemoveAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<byte[]>.Some([1])));

        var result = await _handler.HandleAsync(
            new CompleteAddPasskeyCommand("valid-token", "{}"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        // Existing passkey preserved + new one added = 2
        Assert.Equal(2, user.Passkeys.Count);
        Assert.Contains(user.Passkeys, pk => pk.CredentialId is [9]);
        Assert.Contains(user.Passkeys, pk => pk.CredentialId is [1]);
        await _userRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_InvalidToken_ReturnsValidationError()
    {
        _challengeStore.GetAndRemoveAsync("add-passkey:bad-token", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<byte[]>.None));

        var result = await _handler.HandleAsync(
            new CompleteAddPasskeyCommand("bad-token", "{}"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("add_passkey_token.invalid", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsNotFound()
    {
        _challengeStore.GetAndRemoveAsync("add-passkey:valid-token", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<byte[]>.Some(
                System.Text.Encoding.UTF8.GetBytes("test@example.com"))));
        _userRepo.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.None));

        var result = await _handler.HandleAsync(
            new CompleteAddPasskeyCommand("valid-token", "{}"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("user.not_found", result.Error.Code);
    }
}