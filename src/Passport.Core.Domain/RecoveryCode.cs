using Supercluster.Lib.Domain;

namespace Passport.Core.Domain;

public sealed class RecoveryCode : Entity, IEquatable<RecoveryCode>
{
    // ------------------------------------------------------------
    // Constructors & Factories
    // ------------------------------------------------------------

    private RecoveryCode() { }

    /// <summary>
    /// Issues a new recovery code. The caller is responsible for generating the
    /// plaintext code, hashing it, and passing <paramref name="codeHash"/>.
    /// </summary>
    public static RecoveryCode Issue(RecoveryCodeId id, string codeHash, RecoveryCodePurpose purpose, DateTimeOffset now, TimeSpan ttl)
    {
        if (string.IsNullOrWhiteSpace(codeHash))
        {
            throw new ArgumentException("Code hash must not be empty.", nameof(codeHash));
        }

        if (ttl <= TimeSpan.Zero)
        {
            throw new ArgumentException("TTL must be positive.", nameof(ttl));
        }

        return new RecoveryCode
        {
            Id = id,
            CodeHash = codeHash,
            Purpose = purpose,
            ExpiresAt = now + ttl,
            CreatedAt = now,
        };
    }

    // ------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------

    public RecoveryCodeId Id { get; private set; }

    public string CodeHash { get; private set; } = string.Empty;

    public RecoveryCodePurpose Purpose { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UsedAt { get; private set; }

    public bool IsUsed => UsedAt.HasValue;

    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;

    // ------------------------------------------------------------
    // Equality
    // ------------------------------------------------------------

    public bool Equals(RecoveryCode? other) => other is not null && Id == other.Id;

    public override bool Equals(object? obj) => obj is RecoveryCode other && Equals(other);

    public override int GetHashCode() => Id.GetHashCode();

    // ------------------------------------------------------------
    // Behaviors
    // ------------------------------------------------------------

    /// <summary>
    /// Marks this code as used. Throws if the code has already been used.
    /// </summary>
    public void MarkUsed(DateTimeOffset now)
    {
        if (IsUsed)
        {
            throw new InvalidOperationException("Recovery code has already been used.");
        }

        UsedAt = now;
    }
}