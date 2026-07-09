namespace Passport.Core.Application.Configuration;

public sealed class SigningKeyConfiguration
{
    /// <summary>
    /// JWT signing algorithm (e.g., "ES256" for ECDSA P-256).
    /// </summary>
    public string Algorithm { get; set; } = "ES256";

    /// <summary>
    /// Path to the PEM-encoded signing key file. Created automatically if missing.
    /// </summary>
    public string KeyPath { get; set; } = "signing-key.pem";
}