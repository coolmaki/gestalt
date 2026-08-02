using NSubstitute;
using Xunit;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Commands;
using Passport.Core.Application.ReadModels;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;

namespace Passport.Core.Application.Tests.Commands.Credentials;

/// <summary>
/// Tests for <see cref="BeginAddPasskeyCommandHandler"/>.
/// Phase A (email verification) — step 1 of 4 in the add-passkey flow.
/// </summary>
public class BeginAddPasskeyCommandHandlerTests
{
    private readonly IUserQueryRepository _userQueryRepo = Substitute.For<IUserQueryRepository>();
    private readonly IRecoveryCodeRepository _recoveryCodeRepo = Substitute.For<IRecoveryCodeRepository>();
    private readonly ICodeDeliveryService _codeDelivery = Substitute.For<ICodeDeliveryService>();
    private readonly IGuidProvider _guids = Substitute.For<IGuidProvider>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly BeginAddPasskeyCommandHandler _handler;

    public BeginAddPasskeyCommandHandlerTests()
    {
        _clock.UtcNow().Returns(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        _guids.NewGuid().Returns(Guid.NewGuid());
        _handler = new BeginAddPasskeyCommandHandler(_userQueryRepo, _recoveryCodeRepo, _codeDelivery, _guids, _clock);
    }

    [Fact]
    public async Task HandleAsync_ValidVerifiedEmail_SendsCode()
    {
        _userQueryRepo.FindByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<UserReadModel>.Some(
                new UserReadModel("test@example.com", 1, "2026-01-01", "2026-01-01"))));

        var result = await _handler.HandleAsync(new BeginAddPasskeyCommand("test@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _codeDelivery.Received(1).SendDeviceVerificationCodeAsync(Arg.Any<Domain.ValueObjects.Email>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsSilentSuccess()
    {
        _userQueryRepo.FindByEmailAsync("unknown@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<UserReadModel>.None));

        var result = await _handler.HandleAsync(new BeginAddPasskeyCommand("unknown@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HandleAsync_UnverifiedEmail_ReturnsSilentSuccess()
    {
        _userQueryRepo.FindByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<UserReadModel>.Some(
                new UserReadModel("test@example.com", 0, "2026-01-01", "2026-01-01"))));

        var result = await _handler.HandleAsync(new BeginAddPasskeyCommand("test@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}