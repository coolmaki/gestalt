using Supercluster.Lib.Domain;
using Supercluster.Lib.Primitives;
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
    public static Result<User> Register(Email email, DateTimeOffset now)
    {
        var user = new User
        {
            Email = email,
            EmailVerified = false,
            CreatedAt = now,
            UpdatedAt = now,
        };

        user.RaiseEvent(new UserRegistered(email.Value, now));

        return user;
    }

    // ------------------------------------------------------------
    // Backing Fields
    // ------------------------------------------------------------

    private readonly List<PasskeyCredential> _passkeys = [];

    // ------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------

    public Email Email { get; private set; } = null!;

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
    public Result<PasskeyCredential> AddPasskey(byte[] credentialId, byte[] publicKey, uint signCount, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(credentialId);
        ArgumentNullException.ThrowIfNull(publicKey);

        var passkeyResult = PasskeyCredential.Create(credentialId, publicKey, signCount, now);
        if (passkeyResult.IsFailure)
        {
            return passkeyResult.Error;
        }

        var passkey = passkeyResult.Value;
        _passkeys.Add(passkey);
        UpdatedAt = now;
        RaiseEvent(new PasskeyAdded(Email.Value, credentialId, now));
        return passkey;
    }

    /// <summary>
    /// Removes a passkey credential. Returns an error if the credential is not found.
    /// The caller must enforce that at least one passkey remains (or that the
    /// user is in a recovery flow).
    /// </summary>
    public Result<Unit> RemovePasskey(byte[] credentialId, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(credentialId);

        var passkey = _passkeys.FirstOrDefault(p => p.CredentialId.SequenceEqual(credentialId));
        if (passkey is null)
        {
            return Error.NotFound("passkey.not_found", "Passkey credential not found.");
        }

        _passkeys.Remove(passkey);
        UpdatedAt = now;
        RaiseEvent(new PasskeyRemoved(Email.Value, credentialId, now));
        return Unit.Value;
    }

    // ------------------------------------------------------------
    // Internal
    // ------------------------------------------------------------

    internal Result<Unit> VerifyEmail(DateTimeOffset now)
    {
        if (EmailVerified)
        {
            return Unit.Value;
        }

        EmailVerified = true;
        UpdatedAt = now;
        RaiseEvent(new EmailVerified(Email.Value, now));
        return Unit.Value;
    }
}