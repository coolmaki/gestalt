using NSubstitute;
using Xunit;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Commands;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Tests.Commands.Recovery;

public class VerifyRecoveryCodeCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidCode_ReturnsRecoveryToken()
    {
        var recoveryCodeRepo = Substitute.For<IRecoveryCodeRepository>();
        var challengeStore = Substitute.For<IChallengeStore>();
        var clock = Substitute.For<IDateTimeProvider>();
        clock.UtcNow().Returns(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

        var email = Email.Create("test@example.com").Value;
        string plainCode = "123456";
        string codeHash = Helpers.HashCode(plainCode);
        var recoveryCode = RecoveryCode.Issue(new RecoveryCodeId(Guid.NewGuid()), email, codeHash, RecoveryCodePurpose.AccountRecovery, clock.UtcNow(), TimeSpan.FromMinutes(10)).Value;

        recoveryCodeRepo.FindActiveByEmailAsync(email, RecoveryCodePurpose.AccountRecovery, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<RecoveryCode>.Some(recoveryCode)));

        var handler = new VerifyRecoveryCodeCommandHandler(recoveryCodeRepo, challengeStore, clock);

        var result = await handler.HandleAsync(new VerifyRecoveryCodeCommand("test@example.com", plainCode), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.RecoveryToken);
    }

    [Fact]
    public async Task HandleAsync_WrongCode_ReturnsValidationError()
    {
        var recoveryCodeRepo = Substitute.For<IRecoveryCodeRepository>();
        var clock = Substitute.For<IDateTimeProvider>();
        clock.UtcNow().Returns(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

        var email = Email.Create("test@example.com").Value;
        var recoveryCode = RecoveryCode.Issue(new RecoveryCodeId(Guid.NewGuid()), email, Helpers.HashCode("654321"), RecoveryCodePurpose.AccountRecovery, clock.UtcNow(), TimeSpan.FromMinutes(10)).Value;

        recoveryCodeRepo.FindActiveByEmailAsync(email, RecoveryCodePurpose.AccountRecovery, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<RecoveryCode>.Some(recoveryCode)));

        var handler = new VerifyRecoveryCodeCommandHandler(recoveryCodeRepo, Substitute.For<IChallengeStore>(), clock);

        var result = await handler.HandleAsync(new VerifyRecoveryCodeCommand("test@example.com", "000000"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("recovery_code.invalid", result.Error.Code);
    }
}