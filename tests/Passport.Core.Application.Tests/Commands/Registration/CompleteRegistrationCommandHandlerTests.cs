using NSubstitute;
using Xunit;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Commands;
using Passport.Core.Application.ReadModels;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;
using Passport.Core.Domain.Entities;

namespace Passport.Core.Application.Tests.Commands.Registration;

public class CompleteRegistrationCommandHandlerTests
{
    private readonly IUserCommandRepository _userRepo = Substitute.For<IUserCommandRepository>();
    private readonly IUserQueryRepository _userQueryRepo = Substitute.For<IUserQueryRepository>();
    private readonly IRecoveryCodeRepository _recoveryCodeRepo = Substitute.For<IRecoveryCodeRepository>();
    private readonly IChallengeStore _challengeStore = Substitute.For<IChallengeStore>();
    private readonly IFido2 _fido2 = Substitute.For<IFido2>();
    private readonly ICodeDeliveryService _codeDelivery = Substitute.For<ICodeDeliveryService>();
    private readonly IGuidProvider _guids = Substitute.For<IGuidProvider>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly CompleteRegistrationCommandHandler _handler;

    public CompleteRegistrationCommandHandlerTests()
    {
        _clock.UtcNow().Returns(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        _guids.NewGuid().Returns(Guid.NewGuid());
        _handler = new CompleteRegistrationCommandHandler(_userRepo, _userQueryRepo, _recoveryCodeRepo, _challengeStore, _fido2, _codeDelivery, _guids, _clock);
    }

    [Fact]
    public async Task HandleAsync_NewEmailAndValidAttestation_ReturnsSuccess()
    {
        var command = new CompleteRegistrationCommand("test@example.com", "fake-attestation");
        _userQueryRepo.FindByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<UserReadModel>.None));
        _challengeStore.GetAndRemoveAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<byte[]>.Some(new byte[] { 1, 2, 3 })));
        _fido2.CompleteRegistrationAsync(Arg.Any<byte[]>(), "fake-attestation", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<(byte[], byte[], uint)>.Success(([4], [5], 0))));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _userRepo.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _userRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _codeDelivery.Received(1).SendVerificationCodeAsync(Arg.Any<Domain.ValueObjects.Email>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_DuplicateEmail_ReturnsConflict()
    {
        var command = new CompleteRegistrationCommand("test@example.com", "fake");
        _userQueryRepo.FindByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<UserReadModel>.Some(
                new UserReadModel("test@example.com", 1, "2026-01-01", "2026-01-01"))));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("email.already_registered", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_ChallengeExpired_ReturnsValidationError()
    {
        var command = new CompleteRegistrationCommand("test@example.com", "fake");
        _userQueryRepo.FindByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<UserReadModel>.None));
        _challengeStore.GetAndRemoveAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<byte[]>.None));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("challenge.expired", result.Error.Code);
    }
}