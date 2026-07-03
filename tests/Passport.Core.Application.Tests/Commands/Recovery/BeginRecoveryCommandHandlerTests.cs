using NSubstitute;
using Xunit;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Commands;
using Passport.Core.Application.ReadModels;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;

namespace Passport.Core.Application.Tests.Commands.Recovery;

public class BeginRecoveryCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidVerifiedEmail_SendsCode()
    {
        var userQueryRepo = Substitute.For<IUserQueryRepository>();
        var emailSender = Substitute.For<IEmailSender>();
        var guids = Substitute.For<IGuidProvider>();
        var clock = Substitute.For<IDateTimeProvider>();
        clock.UtcNow().Returns(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        guids.NewGuid().Returns(Guid.NewGuid());

        var handler = new BeginRecoveryCommandHandler(userQueryRepo, Substitute.For<IRecoveryCodeRepository>(), emailSender, guids, clock);

        userQueryRepo.FindByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<UserReadModel>.Some(
                new UserReadModel("test@example.com", 1, "2026-01-01", "2026-01-01"))));

        var result = await handler.HandleAsync(new BeginRecoveryCommand("test@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await emailSender.Received(1).SendRecoveryCodeAsync(Arg.Any<Domain.ValueObjects.Email>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsSilentSuccess()
    {
        var userQueryRepo = Substitute.For<IUserQueryRepository>();
        var handler = new BeginRecoveryCommandHandler(
            userQueryRepo,
            Substitute.For<IRecoveryCodeRepository>(),
            Substitute.For<IEmailSender>(),
            Substitute.For<IGuidProvider>(),
            Substitute.For<IDateTimeProvider>());

        userQueryRepo.FindByEmailAsync("unknown@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<UserReadModel>.None));

        var result = await handler.HandleAsync(new BeginRecoveryCommand("unknown@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HandleAsync_UnverifiedEmail_ReturnsSilentSuccess()
    {
        var userQueryRepo = Substitute.For<IUserQueryRepository>();
        userQueryRepo.FindByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<UserReadModel>.Some(
                new UserReadModel("test@example.com", 0, "2026-01-01", "2026-01-01"))));

        var handler = new BeginRecoveryCommandHandler(
            userQueryRepo, Substitute.For<IRecoveryCodeRepository>(), Substitute.For<IEmailSender>(),
            Substitute.For<IGuidProvider>(), Substitute.For<IDateTimeProvider>());

        var result = await handler.HandleAsync(new BeginRecoveryCommand("test@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}