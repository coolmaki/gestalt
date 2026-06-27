using Supercluster.Lib.Domain;

namespace Passport.Core.Domain;

public sealed class PasskeyCredential : Entity, IEquatable<PasskeyCredential>
{
    // ------------------------------------------------------------
    // Constructors & Factories
    // ------------------------------------------------------------

    private PasskeyCredential() { }

    internal static PasskeyCredential Create(byte[] credentialId, byte[] publicKey, uint signCount, DateTimeOffset now)
    {
        if (credentialId is null || credentialId.Length == 0)
        {
            throw new ArgumentException("Credential ID must not be empty.", nameof(credentialId));
        }

        if (publicKey is null || publicKey.Length == 0)
        {
            throw new ArgumentException("Public key must not be empty.", nameof(publicKey));
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

    public string? DeviceName { get; private set; }

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

    internal void SetDeviceName(string? name)
    {
        DeviceName = name;
    }
}