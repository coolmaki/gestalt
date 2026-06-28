using Xunit;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Domain.Tests.Entities;

public class RecoveryCodeTests
{
    private readonly Email _email;
    private readonly DateTimeOffset _now;
    private readonly TimeSpan _ttl;

    public RecoveryCodeTests()
    {
        _email = Email.Create("test@example.com").Value;
        _now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _ttl = TimeSpan.FromMinutes(10);
    }

    [Fact]
    public void Issue_ValidInput_CreatesRecoveryCode()
    {
        var id = new RecoveryCodeId(Guid.NewGuid());

        var result = RecoveryCode.Issue(id, _email, "abc123hash", RecoveryCodePurpose.EmailVerification, _now, _ttl);

        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value.Id);
        Assert.Equal(_email, result.Value.Email);
        Assert.Equal(RecoveryCodePurpose.EmailVerification, result.Value.Purpose);
        Assert.Equal(_now + _ttl, result.Value.ExpiresAt);
        Assert.False(result.Value.IsUsed);
    }

    [Fact]
    public void Issue_EmptyCodeHash_ReturnsValidationError()
    {
        var result = RecoveryCode.Issue(new RecoveryCodeId(Guid.NewGuid()), _email, "", RecoveryCodePurpose.EmailVerification, _now, _ttl);

        Assert.True(result.IsFailure);
        Assert.Equal("code_hash.empty", result.Error.Code);
    }

    [Fact]
    public void Issue_ZeroTtl_ReturnsValidationError()
    {
        var result = RecoveryCode.Issue(new RecoveryCodeId(Guid.NewGuid()), _email, "hash", RecoveryCodePurpose.EmailVerification, _now, TimeSpan.Zero);

        Assert.True(result.IsFailure);
        Assert.Equal("ttl.invalid", result.Error.Code);
    }

    [Fact]
    public void MarkUsed_UnusedCode_MarksAsUsed()
    {
        var code = RecoveryCode.Issue(new RecoveryCodeId(Guid.NewGuid()), _email, "hash", RecoveryCodePurpose.AccountRecovery, _now, _ttl).Value;

        var result = code.MarkUsed(_now);

        Assert.True(result.IsSuccess);
        Assert.True(code.IsUsed);
    }

    [Fact]
    public void MarkUsed_AlreadyUsed_ReturnsConflict()
    {
        var code = RecoveryCode.Issue(new RecoveryCodeId(Guid.NewGuid()), _email, "hash", RecoveryCodePurpose.AccountRecovery, _now, _ttl).Value;
        code.MarkUsed(_now);

        var result = code.MarkUsed(_now);

        Assert.True(result.IsFailure);
        Assert.Equal("recovery_code.already_used", result.Error.Code);
    }

    [Fact]
    public void IsExpired_BeforeExpiry_ReturnsFalse()
    {
        var code = RecoveryCode.Issue(new RecoveryCodeId(Guid.NewGuid()), _email, "hash", RecoveryCodePurpose.EmailVerification, _now, _ttl).Value;

        Assert.False(code.IsExpired(_now));
    }

    [Fact]
    public void IsExpired_AfterExpiry_ReturnsTrue()
    {
        var code = RecoveryCode.Issue(new RecoveryCodeId(Guid.NewGuid()), _email, "hash", RecoveryCodePurpose.EmailVerification, _now, _ttl).Value;

        Assert.True(code.IsExpired(_now + TimeSpan.FromMinutes(11)));
    }

    [Fact]
    public void Equality_SameId_AreEqual()
    {
        var id = new RecoveryCodeId(Guid.NewGuid());
        var a = RecoveryCode.Issue(id, _email, "hash1", RecoveryCodePurpose.EmailVerification, _now, _ttl).Value;
        var b = RecoveryCode.Issue(id, _email, "hash2", RecoveryCodePurpose.AccountRecovery, _now, _ttl).Value;

        Assert.Equal(a, b);
    }
}