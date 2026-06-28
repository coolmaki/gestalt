using Supercluster.Lib.Domain;
using Passport.Core.Domain.Events;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Domain.Entities;

public sealed class User : AggregateRoot, IEquatable<User>
{
    // ------------------------------------------------------------
    // Constructors & Factories
    // ------------------------------------------------------------

    private User() { }

    /// <summary>
    /// Registers a new user with an email address. Email is not yet verified —
    /// the caller must send a verification email and call <see cref="VerifyEmail"/>.
    /// </summary>
    public static User Register(string email, UserId userId, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email must not be empty.", nameof(email));
        }

        string normalizedEmail = email.Trim().ToLowerInvariant();

        var user = new User
        {
            Id = userId,
            Email = normalizedEmail,
            EmailVerified = false,
            CreatedAt = now,
            UpdatedAt = now,
        };

        user.RaiseEvent(new UserRegistered(normalizedEmail, now));

        return user;
    }

    // ------------------------------------------------------------
    // Backing Fields
    // ------------------------------------------------------------

    private readonly List<PasskeyCredential> _passkeys = [];

    // ------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------

    public UserId Id { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public bool EmailVerified { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<PasskeyCredential> Passkeys => _passkeys.AsReadOnly();

    // ------------------------------------------------------------
    // Equality
    // ------------------------------------------------------------

    public bool Equals(User? other) => other is not null && Email == other.Email;

    public override bool Equals(object? obj) => obj is User other && Equals(other);

    public override int GetHashCode() => Email.GetHashCode();

    // ------------------------------------------------------------
    // Behaviors
    // ------------------------------------------------------------

    /// <summary>
    /// Adds a new passkey credential to the user. The caller is responsible for
    /// validating the WebAuthn attestation before calling this.
    /// </summary>
    public PasskeyCredential AddPasskey(byte[] credentialId, byte[] publicKey, uint signCount, DateTimeOffset now)
    {
        var passkey = PasskeyCredential.Create(credentialId, publicKey, signCount, now);
        _passkeys.Add(passkey);
        UpdatedAt = now;
        RaiseEvent(new PasskeyAdded(Email, credentialId, now));
        return passkey;
    }

    /// <summary>
    /// Removes a passkey credential. Fails if the credential is not found.
    /// The caller must enforce that at least one passkey remains (or that the
    /// user is in a recovery flow).
    /// </summary>
    public void RemovePasskey(byte[] credentialId, DateTimeOffset now)
    {
        var passkey = _passkeys.FirstOrDefault(p => p.CredentialId.SequenceEqual(credentialId))
            ?? throw new InvalidOperationException("Passkey credential not found.");

        _passkeys.Remove(passkey);
        UpdatedAt = now;
        RaiseEvent(new PasskeyRemoved(Email, credentialId, now));
    }

    // ------------------------------------------------------------
    // Internal
    // ------------------------------------------------------------

    internal void VerifyEmail(DateTimeOffset now)
    {
        if (EmailVerified)
        {
            return;
        }

        EmailVerified = true;
        UpdatedAt = now;
        RaiseEvent(new EmailVerified(Email, now));
    }
}