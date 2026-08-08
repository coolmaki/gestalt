using Gestalt.Lib.Domain;
using Gestalt.Lib.Primitives;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Domain.Entities;

public sealed class RecoveryCode : Entity, IEquatable<RecoveryCode>
{
    // ------------------------------------------------------------
    // Constructors & Factories
    // ------------------------------------------------------------

    private RecoveryCode() { }

    /// <summary>
    /// Issues a new recovery code for the given email. The caller is responsible
    /// for generating the plaintext code, hashing it, and passing <paramref name="codeHash"/>.
    /// </summary>
    public static Result<RecoveryCode> Issue(RecoveryCodeId id, Email email, string codeHash, RecoveryCodePurpose purpose, DateTimeOffset now, TimeSpan ttl)
    {
        if (string.IsNullOrWhiteSpace(codeHash))
        {
            return Error.Validation("code_hash.empty", "Code hash must not be empty.");
        }

        if (ttl <= TimeSpan.Zero)
        {
            return Error.Validation("ttl.invalid", "TTL must be positive.");
        }

        return new RecoveryCode
        {
            Id = id,
            Email = email,
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

    public Email Email { get; private set; } = null!;

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
    /// Marks this code as used. Returns an error if the code has already been used.
    /// </summary>
    public Result<Unit> MarkUsed(DateTimeOffset now)
    {
        if (IsUsed)
        {
            return Error.Conflict("recovery_code.already_used", "Recovery code has already been used.");
        }

        UsedAt = now;
        return Unit.Value;
    }
}