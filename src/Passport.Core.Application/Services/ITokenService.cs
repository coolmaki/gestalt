namespace Passport.Core.Application.Services;

/// <summary>
/// Generates JWT access tokens and opaque refresh tokens, and exposes
/// the signing key for the JWKS endpoint.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a signed JWT access token for the given user.
    /// </summary>
    string GenerateAccessToken(string email);

    /// <summary>
    /// Generates a cryptographically random refresh token.
    /// Returns the raw token (to send to the client) and its SHA-256 hash (for storage).
    /// </summary>
    (string rawToken, string tokenHash) GenerateRefreshToken();

    /// <summary>
    /// Returns the public signing key for the JWKS endpoint.
    /// </summary>
    JwksKey GetSigningKey();
}