using NSubstitute;
using Xunit;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Commands;
using Passport.Core.Application.Repositories;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Tests.Commands.Verification;

public class VerifyEmailCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidCode_VerifiesEmail()
    {
        var userRepo = Substitute.For<IUserCommandRepository>();
        var recoveryCodeRepo = Substitute.For<IRecoveryCodeRepository>();
        var clock = Substitute.For<IDateTimeProvider>();
        clock.UtcNow().Returns(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

        var email = Email.Create("test@example.com").Value;
        var user = User.Register(email, clock.UtcNow()).Value;

        string plainCode = "123456";
        string codeHash = Helpers.HashCode(plainCode);
        var recoveryCode = RecoveryCode.Issue(new RecoveryCodeId(Guid.NewGuid()), email, codeHash, RecoveryCodePurpose.EmailVerification, clock.UtcNow(), TimeSpan.FromMinutes(10)).Value;

        userRepo.FindByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.Some(user)));
        recoveryCodeRepo.FindActiveByEmailAsync(email, RecoveryCodePurpose.EmailVerification, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<RecoveryCode>.Some(recoveryCode)));

        var handler = new VerifyEmailCommandHandler(userRepo, recoveryCodeRepo, clock);

        var result = await handler.HandleAsync(new VerifyEmailCommand("test@example.com", plainCode), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(user.EmailVerified);
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsNotFound()
    {
        var userRepo = Substitute.For<IUserCommandRepository>();
        userRepo.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.None));

        var handler = new VerifyEmailCommandHandler(userRepo, Substitute.For<IRecoveryCodeRepository>(), Substitute.For<IDateTimeProvider>());

        var result = await handler.HandleAsync(new VerifyEmailCommand("test@example.com", "123456"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("user.not_found", result.Error.Code);
    }
}