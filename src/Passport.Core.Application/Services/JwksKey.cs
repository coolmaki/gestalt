namespace Passport.Core.Application.Services;

public sealed record JwksKey(
    string Kty,
    string Alg,
    string Kid,
    string X,
    string Y,
    string Crv
);