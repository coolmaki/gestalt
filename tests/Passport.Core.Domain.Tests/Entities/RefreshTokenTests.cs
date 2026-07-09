using Xunit;
using Passport.Core.Domain.Entities;

namespace Passport.Core.Domain.Tests.Entities;

public class RefreshTokenTests
{
    private readonly DateTimeOffset _now;
    private readonly TimeSpan _ttl;

    public RefreshTokenTests()
    {
        _now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _ttl = TimeSpan.FromDays(30);
    }

    [Fact]
    public void Issue_ValidParams_CreatesToken()
    {
        var token = RefreshToken.Issue("abc123", _now, _ttl);

        Assert.Equal("abc123", token.TokenHash);
        Assert.Equal(_now, token.IssuedAt);
        Assert.Equal(_now + _ttl, token.ExpiresAt);
        Assert.False(token.IsRevoked);
        Assert.False(token.IsExpired(_now));
    }

    [Fact]
    public void Issue_WithClientId_SetsClientId()
    {
        var token = RefreshToken.Issue("hash", _now, _ttl, "client-1");

        Assert.Equal("client-1", token.ClientId);
    }

    [Fact]
    public void Issue_WithoutClientId_ClientIdIsNull()
    {
        var token = RefreshToken.Issue("hash", _now, _ttl);

        Assert.Null(token.ClientId);
    }

    [Fact]
    public void Revoke_NotRevoked_SetsRevokedAt()
    {
        var token = RefreshToken.Issue("hash", _now, _ttl);

        token.Revoke(_now);

        Assert.True(token.IsRevoked);
        Assert.Equal(_now, token.RevokedAt);
    }

    [Fact]
    public void Revoke_AlreadyRevoked_ReturnsConflict()
    {
        var token = RefreshToken.Issue("hash", _now, _ttl);
        token.Revoke(_now);

        var result = token.Revoke(_now);

        Assert.True(result.IsFailure);
        Assert.Equal("token.already_revoked", result.Error.Code);
    }

    [Fact]
    public void IsExpired_BeforeExpiry_ReturnsFalse()
    {
        var token = RefreshToken.Issue("hash", _now, _ttl);

        Assert.False(token.IsExpired(_now));
    }

    [Fact]
    public void IsExpired_AfterExpiry_ReturnsTrue()
    {
        var token = RefreshToken.Issue("hash", _now, _ttl);

        Assert.True(token.IsExpired(_now + _ttl + TimeSpan.FromTicks(1)));
    }

    [Fact]
    public void Equals_SameTokenHash_ReturnsTrue()
    {
        var token1 = RefreshToken.Issue("hash", _now, _ttl);
        var token2 = RefreshToken.Issue("hash", _now.AddDays(1), _ttl);

        Assert.True(token1.Equals(token2));
    }

    [Fact]
    public void Equals_DifferentTokenHash_ReturnsFalse()
    {
        var token1 = RefreshToken.Issue("hash1", _now, _ttl);
        var token2 = RefreshToken.Issue("hash2", _now, _ttl);

        Assert.False(token1.Equals(token2));
    }

    [Fact]
    public void Equals_NonRefreshTokenObject_ReturnsFalse()
    {
        var token = RefreshToken.Issue("hash", _now, _ttl);

        Assert.False(token.Equals("not-a-token"));
    }

    [Fact]
    public void GetHashCode_SameHash_ReturnsSameValue()
    {
        var token1 = RefreshToken.Issue("hash", _now, _ttl);
        var token2 = RefreshToken.Issue("hash", _now.AddDays(1), _ttl);

        Assert.Equal(token1.GetHashCode(), token2.GetHashCode());
    }
}