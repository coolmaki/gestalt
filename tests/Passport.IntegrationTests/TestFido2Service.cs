using Supercluster.Lib.Primitives;
using Passport.Core.Application.Services;
using Passport.Core.Domain.ValueObjects;

namespace Passport.IntegrationTests;

/// <summary>
/// Deterministic FIDO2 service for integration tests. Bypasses real WebAuthn crypto
/// and returns fixed credentials. The real implementation is tested separately.
/// </summary>
internal sealed class TestFido2Service : IFido2
{
    public Task<Result<(string OptionsJson, byte[] Challenge)>> CreateRegistrationOptionsAsync(Email user, CancellationToken cancellationToken)
    {
        var options = new
        {
            rp = new { name = "Passport" },
            user = new { id = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(user.Value)), name = user.Value, displayName = user.Value },
            challenge = Convert.ToBase64String(Challenge),
            pubKeyCredParams = new[] { new { type = "public-key", alg = -7 } },
        };

        return Task.FromResult(Result<(string, byte[])>.Success((
            System.Text.Json.JsonSerializer.Serialize(options),
            Challenge)));
    }

    public Task<Result<(byte[] CredentialId, byte[] PublicKey, uint SignCount)>> CompleteRegistrationAsync(byte[] challenge, string attestationJson, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<(byte[], byte[], uint)>.Success((
            new byte[] { 0x01, 0x02, 0x03 },
            new byte[] { 0x04, 0x05, 0x06 },
            0)));
    }

    public Task<Result<(string OptionsJson, byte[] Challenge)>> CreateAssertionOptionsAsync(IReadOnlyCollection<byte[]> allowedCredentials, CancellationToken cancellationToken)
    {
        var options = new
        {
            challenge = Convert.ToBase64String(Challenge),
            allowCredentials = allowedCredentials.Select(c => new { type = "public-key", id = Convert.ToBase64String(c) }),
        };

        return Task.FromResult(Result<(string, byte[])>.Success((
            System.Text.Json.JsonSerializer.Serialize(options),
            Challenge)));
    }

    public Task<Result<uint>> CompleteAssertionAsync(byte[] challenge, string assertionJson, byte[] storedPublicKey, uint currentSignCount, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<uint>.Success(currentSignCount + 1));
    }

    private static readonly byte[] Challenge = new byte[] { 0xAA, 0xBB, 0xCC };
}