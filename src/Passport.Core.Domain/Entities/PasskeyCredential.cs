using Gestalt.Lib.Domain;
using Gestalt.Lib.Primitives;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Domain.Entities;

public sealed class PasskeyCredential : Entity, IEquatable<PasskeyCredential>
{
    // ------------------------------------------------------------
    // Constructors & Factories
    // ------------------------------------------------------------

    private PasskeyCredential() { }

    internal static Result<PasskeyCredential> Create(byte[] credentialId, byte[] publicKey, uint signCount, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(credentialId);
        ArgumentNullException.ThrowIfNull(publicKey);

        if (credentialId.Length == 0)
        {
            return Error.Validation("credential_id.empty", "Credential ID must not be empty.");
        }

        if (publicKey.Length == 0)
        {
            return Error.Validation("public_key.empty", "Public key must not be empty.");
        }

        return new PasskeyCredential
        {
            CredentialId = credentialId,
            PublicKey = publicKey,
            SignCount = signCount,
            CreatedAt = now,
        };
    }

    // ------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------

    public byte[] CredentialId { get; private set; } = [];

    public byte[] PublicKey { get; private set; } = [];

    public uint SignCount { get; private set; }

    public DeviceName? DeviceName { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    // ------------------------------------------------------------
    // Equality
    // ------------------------------------------------------------

    public bool Equals(PasskeyCredential? other) =>
        other is not null && CredentialId.SequenceEqual(other.CredentialId);

    public override bool Equals(object? obj) => obj is PasskeyCredential other && Equals(other);

    public override int GetHashCode() => CredentialId.Aggregate(0, (hash, b) => HashCode.Combine(hash, b));

    // ------------------------------------------------------------
    // Internal
    // ------------------------------------------------------------

    internal void UpdateSignCount(uint newCount)
    {
        SignCount = newCount;
    }

    internal Result<Unit> SetDeviceName(DeviceName name)
    {
        DeviceName = name;
        return Unit.Value;
    }
}