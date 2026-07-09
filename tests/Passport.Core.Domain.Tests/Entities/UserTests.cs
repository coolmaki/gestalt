using Xunit;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Domain.Tests.Entities;

public class UserTests
{
    private readonly Email _email;
    private readonly DateTimeOffset _now;

    public UserTests()
    {
        _email = Email.Create("test@example.com").Value;
        _now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    }

    [Fact]
    public void Register_ValidEmail_CreatesUser()
    {
        var result = User.Register(_email, _now);

        Assert.True(result.IsSuccess);
        Assert.Equal(_email, result.Value.Email);
        Assert.False(result.Value.EmailVerified);
        Assert.Equal(_now, result.Value.CreatedAt);
        Assert.Empty(result.Value.Passkeys);
    }

    [Fact]
    public void Register_RaisesUserRegisteredEvent()
    {
        var result = User.Register(_email, _now);

        var user = result.Value;
        Assert.Single(user.Events);
        Assert.IsType<Events.UserRegistered>(user.Events.First());
    }

    [Fact]
    public void AddPasskey_ValidCredential_AddsToCollection()
    {
        var user = User.Register(_email, _now).Value;
        var credentialId = new byte[] { 1, 2, 3 };
        var publicKey = new byte[] { 4, 5, 6 };

        var result = user.AddPasskey(credentialId, publicKey, 0, _now);

        Assert.True(result.IsSuccess);
        Assert.Single(user.Passkeys);
    }

    [Fact]
    public void AddPasskey_RaisesPasskeyAddedEvent()
    {
        var user = User.Register(_email, _now).Value;
        user.ClearEvents();

        user.AddPasskey([1], [2], 0, _now);

        Assert.Single(user.Events);
        Assert.IsType<Events.PasskeyAdded>(user.Events.First());
    }

    [Fact]
    public void RemovePasskey_ExistingCredential_RemovesFromCollection()
    {
        var user = User.Register(_email, _now).Value;
        var credentialId = new byte[] { 1, 2, 3 };
        user.AddPasskey(credentialId, [4], 0, _now);

        var result = user.RemovePasskey(credentialId, _now);

        Assert.True(result.IsSuccess);
        Assert.Empty(user.Passkeys);
    }

    [Fact]
    public void RemovePasskey_NotFound_ReturnsNotFound()
    {
        var user = User.Register(_email, _now).Value;

        var result = user.RemovePasskey([9, 9, 9], _now);

        Assert.True(result.IsFailure);
        Assert.Equal("passkey.not_found", result.Error.Code);
    }

    [Fact]
    public void VerifyEmail_NotVerified_SetsVerified()
    {
        var user = User.Register(_email, _now).Value;

        user.VerifyEmail(_now);

        Assert.True(user.EmailVerified);
    }

    [Fact]
    public void VerifyEmail_AlreadyVerified_DoesNotRaiseAgain()
    {
        var user = User.Register(_email, _now).Value;
        user.VerifyEmail(_now);
        user.ClearEvents();

        user.VerifyEmail(_now);

        Assert.Empty(user.Events);
    }

    [Fact]
    public void Equality_SameEmail_AreEqual()
    {
        var a = User.Register(_email, _now).Value;
        var b = User.Register(_email, _now).Value;

        Assert.Equal(a, b);
    }

    [Fact]
    public void Equality_DifferentEmail_AreNotEqual()
    {
        var a = User.Register(_email, _now).Value;
        var otherEmail = Email.Create("other@example.com").Value;
        var b = User.Register(otherEmail, _now).Value;

        Assert.NotEqual(a, b);
    }

    // --- RefreshToken tests ---

    [Fact]
    public void IssueRefreshToken_AddsToCollection()
    {
        var user = User.Register(_email, _now).Value;

        user.IssueRefreshToken("hash1", _now, TimeSpan.FromDays(30));

        Assert.Single(user.RefreshTokens);
        Assert.Equal("hash1", user.RefreshTokens.First().TokenHash);
    }

    [Fact]
    public void IssueRefreshToken_RaisesRefreshTokenIssuedEvent()
    {
        var user = User.Register(_email, _now).Value;
        user.ClearEvents();

        user.IssueRefreshToken("hash1", _now, TimeSpan.FromDays(30));

        Assert.Single(user.Events);
        Assert.IsType<Events.RefreshTokenIssued>(user.Events.First());
    }

    [Fact]
    public void RevokeRefreshToken_ValidHash_RevokesAndReturnsSuccess()
    {
        var user = User.Register(_email, _now).Value;
        user.IssueRefreshToken("hash1", _now, TimeSpan.FromDays(30));

        var result = user.RevokeRefreshToken("hash1", _now);

        Assert.True(result.IsSuccess);
        Assert.True(user.RefreshTokens.First().IsRevoked);
    }

    [Fact]
    public void RevokeRefreshToken_InvalidHash_ReturnsNotFound()
    {
        var user = User.Register(_email, _now).Value;

        var result = user.RevokeRefreshToken("nonexistent", _now);

        Assert.True(result.IsFailure);
        Assert.Equal("token.not_found", result.Error.Code);
    }

    [Fact]
    public void RevokeAllRefreshTokens_RevokesAll()
    {
        var user = User.Register(_email, _now).Value;
        user.IssueRefreshToken("hash1", _now, TimeSpan.FromDays(30));
        user.IssueRefreshToken("hash2", _now, TimeSpan.FromDays(30));

        user.RevokeAllRefreshTokens(_now);

        Assert.All(user.RefreshTokens, t => Assert.True(t.IsRevoked));
    }

    [Fact]
    public void IssueRefreshToken_UpdatesUpdatedAt()
    {
        var user = User.Register(_email, _now).Value;
        var later = _now.AddDays(1);

        user.IssueRefreshToken("hash", later, TimeSpan.FromDays(30));

        Assert.Equal(later, user.UpdatedAt);
    }

    // --- Null guard tests ---

    [Fact]
    public void AddPasskey_NullCredentialId_ThrowsArgumentNullException()
    {
        var user = User.Register(_email, _now).Value;

        Assert.Throws<ArgumentNullException>(() => user.AddPasskey(null!, [1], 0, _now));
    }

    [Fact]
    public void AddPasskey_NullPublicKey_ThrowsArgumentNullException()
    {
        var user = User.Register(_email, _now).Value;

        Assert.Throws<ArgumentNullException>(() => user.AddPasskey([1], null!, 0, _now));
    }
}