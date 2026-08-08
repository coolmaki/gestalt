using System.Text;
using System.Text.Json;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.Extensions.Options;
using Gestalt.Lib.Primitives;
using Passport.Core.Application.Services;
using Passport.Core.Domain.ValueObjects;
using Passport.Infrastructure.Configuration;

namespace Passport.Infrastructure.Services;

/// <summary>
/// Implements <see cref="IFido2"/> using the fido2-net-lib library.
/// Handles WebAuthn credential creation, attestation verification,
/// assertion options generation, and assertion verification.
/// </summary>
internal sealed class Fido2Service : Passport.Core.Application.Services.IFido2
{
    private readonly Fido2 _fido2;

    public Fido2Service(IOptions<Fido2Config> options)
    {
        var config = options.Value;

        _fido2 = new Fido2(new Fido2Configuration
        {
            ServerDomain = config.ServerDomain,
            ServerName = config.ServerName,
            Origins = new HashSet<string> { config.Origin },
        });
    }

    // ------------------------------------------------------------
    // Registration
    // ------------------------------------------------------------

    public Task<Result<(string OptionsJson, string InternalState)>> CreateRegistrationOptionsAsync(
        Email user, CancellationToken cancellationToken)
    {
        var options = _fido2.RequestNewCredential(new RequestNewCredentialParams
        {
            User = new Fido2User
            {
                Id = Encoding.UTF8.GetBytes(user.Value),
                Name = user.Value,
                DisplayName = user.Value,
            },
            ExcludeCredentials = [],
            AuthenticatorSelection = new AuthenticatorSelection
            {
                UserVerification = UserVerificationRequirement.Preferred,
            },
            AttestationPreference = AttestationConveyancePreference.None,
        });

        var internalState = options.ToJson();
        var clientJson = $"{{\"publicKey\":{internalState}}}";
        return Task.FromResult(Result<(string, string)>.Success((clientJson, internalState)));
    }

    public async Task<Result<(byte[] CredentialId, byte[] PublicKey, uint SignCount)>> CompleteRegistrationAsync(
        string internalState, string attestationJson, CancellationToken cancellationToken)
    {
        var originalOptions = CredentialCreateOptions.FromJson(internalState);

        var attestation = JsonSerializer.Deserialize<AuthenticatorAttestationRawResponse>(attestationJson)
            ?? throw new InvalidOperationException("Failed to deserialize attestation response.");

        var credential = await _fido2.MakeNewCredentialAsync(
            new MakeNewCredentialParams
            {
                AttestationResponse = attestation,
                OriginalOptions = originalOptions,
                IsCredentialIdUniqueToUserCallback = (args, ct) => Task.FromResult(true),
            },
            cancellationToken);

        if (credential is null)
        {
            return Error.Unexpected("fido2.registration_failed", "Credential verification failed.");
        }

        return (credential.Id, credential.PublicKey, credential.SignCount);
    }

    // ------------------------------------------------------------
    // Authentication
    // ------------------------------------------------------------

    public Task<Result<(string OptionsJson, string InternalState)>> CreateAssertionOptionsAsync(
        IReadOnlyCollection<byte[]> allowedCredentials, CancellationToken cancellationToken)
    {
        var options = _fido2.GetAssertionOptions(new GetAssertionOptionsParams
        {
            AllowedCredentials = allowedCredentials
                .Select(c => new PublicKeyCredentialDescriptor(PublicKeyCredentialType.PublicKey, c))
                .ToList(),
            UserVerification = UserVerificationRequirement.Preferred,
        });

        var internalState = options.ToJson();
        var clientJson = $"{{\"publicKey\":{internalState}}}";
        return Task.FromResult(Result<(string, string)>.Success((clientJson, internalState)));
    }

    public async Task<Result<uint>> CompleteAssertionAsync(
        string internalState, string assertionJson, byte[] storedPublicKey, uint currentSignCount, CancellationToken cancellationToken)
    {
        var originalOptions = AssertionOptions.FromJson(internalState);

        var assertion = JsonSerializer.Deserialize<AuthenticatorAssertionRawResponse>(assertionJson)
            ?? throw new InvalidOperationException("Failed to deserialize assertion response.");

        var result = await _fido2.MakeAssertionAsync(
            new MakeAssertionParams
            {
                AssertionResponse = assertion,
                OriginalOptions = originalOptions,
                StoredPublicKey = storedPublicKey,
                StoredSignatureCounter = currentSignCount,
                IsUserHandleOwnerOfCredentialIdCallback = (args, ct) => Task.FromResult(true),
            },
            cancellationToken);

        return result.SignCount;
    }
}