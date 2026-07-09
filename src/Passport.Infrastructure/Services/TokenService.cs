using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Passport.Core.Application.Configuration;
using Passport.Core.Application.Services;

namespace Passport.Infrastructure.Services;

internal sealed class TokenService : ITokenService
{
    private readonly ECDsa _ecdsa;
    private readonly ECDsaSecurityKey _securityKey;
    private readonly JwksKey _jwk;
    private readonly AccessTokenConfiguration _accessTokenConfig;
    private readonly SigningKeyConfiguration _signingConfig;

    public TokenService(ECDsa ecdsa, ECDsaSecurityKey securityKey, ApplicationConfiguration appConfig)
    {
        _ecdsa = ecdsa;
        _securityKey = securityKey;
        _accessTokenConfig = appConfig.AccessToken;
        _signingConfig = appConfig.SigningKey;

        var ecParams = ecdsa.ExportParameters(false);
        _jwk = new JwksKey(
            Kty: "EC",
            Alg: _signingConfig.Algorithm,
            Kid: _securityKey.KeyId,
            X: Base64UrlEncoder.Encode(ecParams.Q.X!),
            Y: Base64UrlEncoder.Encode(ecParams.Q.Y!),
            Crv: "P-256"
        );
    }

    public string GenerateAccessToken(string email)
    {
        var now = DateTimeOffset.UtcNow;
        var signingCredentials = new SigningCredentials(_securityKey, _signingConfig.Algorithm);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(
            [
                new("sub", email),
                new("email", email),
            ]),
            Issuer = _accessTokenConfig.Issuer,
            Audience = _accessTokenConfig.Audience,
            IssuedAt = now.UtcDateTime,
            Expires = now.AddMinutes(_accessTokenConfig.LifetimeMinutes).UtcDateTime,
            SigningCredentials = signingCredentials,
        };

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var token = handler.CreateJwtSecurityToken(tokenDescriptor);
        return handler.WriteToken(token);
    }

    public (string rawToken, string tokenHash) GenerateRefreshToken()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        var rawToken = Convert.ToBase64String(bytes);
        var tokenHash = Convert.ToHexStringLower(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken)));
        return (rawToken, tokenHash);
    }

    public JwksKey GetSigningKey() => _jwk;

    public void Dispose() => _ecdsa.Dispose();

    public static ECDsa LoadOrCreateKey(string keyPath)
    {
        if (File.Exists(keyPath))
        {
            var pem = File.ReadAllText(keyPath);
            var ecdsa = ECDsa.Create();
            ecdsa.ImportFromPem(pem);
            return ecdsa;
        }

        var newKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var newPem = newKey.ExportPkcs8PrivateKeyPem();
        File.WriteAllText(keyPath, newPem);
        return newKey;
    }

    public static string GenerateKeyId()
    {
        var bytes = new byte[8];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexStringLower(bytes);
    }
}