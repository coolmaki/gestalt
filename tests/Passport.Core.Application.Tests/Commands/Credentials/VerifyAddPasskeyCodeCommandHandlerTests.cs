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
/// Tests for <see cref="VerifyAddPasskeyCodeCommandHandler"/>.
/// Phase A (email verification) — step 2 of 4 in the add-passkey flow.
/// </summary>
public class VerifyAddPasskeyCodeCommandHandlerTests
{
    private readonly IRecoveryCodeRepository _recoveryCodeRepo = Substitute.For<IRecoveryCodeRepository>();
    private readonly IChallengeStore _challengeStore = Substitute.For<IChallengeStore>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly VerifyAddPasskeyCodeCommandHandler _handler;

    public VerifyAddPasskeyCodeCommandHandlerTests()
    {
        _clock.UtcNow().Returns(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        _handler = new VerifyAddPasskeyCodeCommandHandler(_recoveryCodeRepo, _challengeStore, _clock);
    }

    [Fact]
    public async Task HandleAsync_ValidCode_ReturnsAddPasskeyToken()
    {
        var email = Email.Create("test@example.com").Value;
        string plainCode = "123456";
        string codeHash = Helpers.HashCode(plainCode);
        var recoveryCode = RecoveryCode.Issue(new RecoveryCodeId(Guid.NewGuid()), email, codeHash, RecoveryCodePurpose.DeviceVerification, _clock.UtcNow(), TimeSpan.FromMinutes(10)).Value;

        _recoveryCodeRepo.FindActiveByEmailAsync(email, RecoveryCodePurpose.DeviceVerification, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<RecoveryCode>.Some(recoveryCode)));

        var result = await _handler.HandleAsync(new VerifyAddPasskeyCodeCommand("test@example.com", plainCode), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.AddPasskeyToken);
    }

    [Fact]
    public async Task HandleAsync_WrongCode_ReturnsValidationError()
    {
        var email = Email.Create("test@example.com").Value;
        var recoveryCode = RecoveryCode.Issue(new RecoveryCodeId(Guid.NewGuid()), email, Helpers.HashCode("654321"), RecoveryCodePurpose.DeviceVerification, _clock.UtcNow(), TimeSpan.FromMinutes(10)).Value;

        _recoveryCodeRepo.FindActiveByEmailAsync(email, RecoveryCodePurpose.DeviceVerification, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<RecoveryCode>.Some(recoveryCode)));

        var result = await _handler.HandleAsync(new VerifyAddPasskeyCodeCommand("test@example.com", "000000"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("verification_code.invalid", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_NoCodeFound_ReturnsValidationError()
    {
        var email = Email.Create("test@example.com").Value;

        _recoveryCodeRepo.FindActiveByEmailAsync(email, RecoveryCodePurpose.DeviceVerification, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<RecoveryCode>.None));

        var result = await _handler.HandleAsync(new VerifyAddPasskeyCodeCommand("test@example.com", "123456"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("verification_code.invalid", result.Error.Code);
    }
}