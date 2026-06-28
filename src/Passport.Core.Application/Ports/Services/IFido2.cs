using Supercluster.Lib.Primitives;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Ports.Services;

/// <summary>
/// Wraps the FIDO2/WebAuthn cryptographic operations.
/// Infrastructure provides fido2-net-lib integration.
/// </summary>
public interface IFido2
{
    /// <summary>
    /// Generates WebAuthn registration options (JSON for navigator.credentials.create).
    /// Returns the options JSON and the raw challenge bytes.
    /// </summary>
    Task<Result<(string OptionsJson, byte[] Challenge)>> CreateRegistrationOptionsAsync(Email user, CancellationToken ct);

    /// <summary>
    /// Validates the attestation response from the client.
    /// Returns the verified credential data.
    /// </summary>
    Task<Result<(byte[] CredentialId, byte[] PublicKey, uint SignCount)>> CompleteRegistrationAsync(byte[] challenge, string attestationJson, CancellationToken ct);

    /// <summary>
    /// Generates WebAuthn assertion options (JSON for navigator.credentials.get).
    /// Returns the options JSON and the raw challenge bytes.
    /// </summary>
    Task<Result<(string OptionsJson, byte[] Challenge)>> CreateAssertionOptionsAsync(IReadOnlyCollection<byte[]> allowedCredentials, CancellationToken ct);

    /// <summary>
    /// Validates the assertion response from the client.
    /// Returns the updated sign count.
    /// </summary>
    Task<Result<uint>> CompleteAssertionAsync(byte[] challenge, string assertionJson, byte[] storedPublicKey, uint currentSignCount, CancellationToken ct);
}