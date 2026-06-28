using Supercluster.Lib.Primitives;
using Passport.Core.Application.Services;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Infrastructure.Services;

/// <summary>
/// Placeholder FIDO2 service. Replace with fido2-net-lib integration.
/// </summary>
internal sealed class Fido2Service : IFido2
{
    public Task<Result<(string OptionsJson, byte[] Challenge)>> CreateRegistrationOptionsAsync(Email user, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<(string, byte[])>.Failure(
            Error.Unexpected("fido2.not_implemented", "FIDO2 service not yet implemented.")));
    }

    public Task<Result<(byte[] CredentialId, byte[] PublicKey, uint SignCount)>> CompleteRegistrationAsync(byte[] challenge, string attestationJson, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<(byte[], byte[], uint)>.Failure(
            Error.Unexpected("fido2.not_implemented", "FIDO2 service not yet implemented.")));
    }

    public Task<Result<(string OptionsJson, byte[] Challenge)>> CreateAssertionOptionsAsync(IReadOnlyCollection<byte[]> allowedCredentials, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<(string, byte[])>.Failure(
            Error.Unexpected("fido2.not_implemented", "FIDO2 service not yet implemented.")));
    }

    public Task<Result<uint>> CompleteAssertionAsync(byte[] challenge, string assertionJson, byte[] storedPublicKey, uint currentSignCount, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<uint>.Failure(
            Error.Unexpected("fido2.not_implemented", "FIDO2 service not yet implemented.")));
    }
}