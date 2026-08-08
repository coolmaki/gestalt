using Gestalt.Lib.Domain;
using Gestalt.Lib.Primitives;

namespace Passport.Core.Domain.Entities;

public sealed class RefreshToken : Entity, IEquatable<RefreshToken>
{
    // ------------------------------------------------------------
    // Constructors & Factories
    // ------------------------------------------------------------

    private RefreshToken() { }

    public static RefreshToken Issue(string tokenHash, DateTimeOffset now, TimeSpan ttl, string? clientId = null)
    {
        return new RefreshToken
        {
            TokenHash = tokenHash,
            ClientId = clientId,
            ExpiresAt = now + ttl,
            IssuedAt = now,
        };
    }

    // ------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------

    public string TokenHash { get; private set; } = string.Empty;

    public string? ClientId { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset IssuedAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public bool IsRevoked => RevokedAt.HasValue;

    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;

    // ------------------------------------------------------------
    // Equality
    // ------------------------------------------------------------

    public bool Equals(RefreshToken? other) => other is not null && TokenHash == other.TokenHash;

    public override bool Equals(object? obj) => obj is RefreshToken other && Equals(other);

    public override int GetHashCode() => TokenHash.GetHashCode();

    // ------------------------------------------------------------
    // Behaviors
    // ------------------------------------------------------------

    public Result<Unit> Revoke(DateTimeOffset now)
    {
        if (IsRevoked)
        {
            return Error.Conflict("token.already_revoked", "Refresh token has already been revoked.");
        }

        RevokedAt = now;
        return Unit.Value;
    }
}