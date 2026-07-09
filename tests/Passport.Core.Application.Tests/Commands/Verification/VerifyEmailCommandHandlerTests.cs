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
    private readonly IUserCommandRepository _userRepo = Substitute.For<IUserCommandRepository>();
    private readonly IRecoveryCodeRepository _recoveryCodeRepo = Substitute.For<IRecoveryCodeRepository>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly VerifyEmailCommandHandler _handler;

    public VerifyEmailCommandHandlerTests()
    {
        _clock.UtcNow().Returns(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        _handler = new VerifyEmailCommandHandler(_userRepo, _recoveryCodeRepo, _clock);
    }

    [Fact]
    public async Task HandleAsync_ValidCode_VerifiesEmail()
    {
        var email = Email.Create("test@example.com").Value;
        var user = User.Register(email, _clock.UtcNow()).Value;

        string plainCode = "123456";
        string codeHash = Helpers.HashCode(plainCode);
        var recoveryCode = RecoveryCode.Issue(new RecoveryCodeId(Guid.NewGuid()), email, codeHash, RecoveryCodePurpose.EmailVerification, _clock.UtcNow(), TimeSpan.FromMinutes(10)).Value;

        _userRepo.FindByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.Some(user)));
        _recoveryCodeRepo.FindActiveByEmailAsync(email, RecoveryCodePurpose.EmailVerification, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<RecoveryCode>.Some(recoveryCode)));

        var result = await _handler.HandleAsync(new VerifyEmailCommand("test@example.com", plainCode), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(user.EmailVerified);
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsNotFound()
    {
        _userRepo.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<User>.None));

        var result = await _handler.HandleAsync(new VerifyEmailCommand("test@example.com", "123456"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("user.not_found", result.Error.Code);
    }
}