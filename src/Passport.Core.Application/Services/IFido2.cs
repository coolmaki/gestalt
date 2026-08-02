using Supercluster.Lib.Primitives;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Services;

/// <summary>
/// Wraps the FIDO2/WebAuthn cryptographic operations.
/// Infrastructure provides fido2-net-lib integration.
/// </summary>
public interface IFido2
{
    /// <summary>
    /// Generates WebAuthn registration options (JSON for navigator.credentials.create).
    /// Returns the options JSON for the client and an opaque <paramref name="InternalState"/>
    /// string that the caller must store and pass back to <see cref="CompleteRegistrationAsync"/>.
    /// </summary>
    Task<Result<(string OptionsJson, string InternalState)>> CreateRegistrationOptionsAsync(Email user, CancellationToken cancellationToken);

    /// <summary>
    /// Validates the attestation response from the client. The <paramref name="internalState"/>
    /// must be the same value returned by a prior call to <see cref="CreateRegistrationOptionsAsync"/>.
    /// Returns the verified credential data.
    /// </summary>
    Task<Result<(byte[] CredentialId, byte[] PublicKey, uint SignCount)>> CompleteRegistrationAsync(string internalState, string attestationJson, CancellationToken cancellationToken);

    /// <summary>
    /// Generates WebAuthn assertion options (JSON for navigator.credentials.get).
    /// Returns the options JSON for the client and an opaque <paramref name="InternalState"/>
    /// string that the caller must store and pass back to <see cref="CompleteAssertionAsync"/>.
    /// </summary>
    Task<Result<(string OptionsJson, string InternalState)>> CreateAssertionOptionsAsync(IReadOnlyCollection<byte[]> allowedCredentials, CancellationToken cancellationToken);

    /// <summary>
    /// Validates the assertion response from the client. The <paramref name="internalState"/>
    /// must be the same value returned by a prior call to <see cref="CreateAssertionOptionsAsync"/>.
    /// Returns the updated sign count.
    /// </summary>
    Task<Result<uint>> CompleteAssertionAsync(string internalState, string assertionJson, byte[] storedPublicKey, uint currentSignCount, CancellationToken cancellationToken);
}