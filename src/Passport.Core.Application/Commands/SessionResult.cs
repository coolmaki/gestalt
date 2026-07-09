namespace Passport.Core.Application.Commands;

public sealed record SessionResult(string AccessToken, string RefreshToken, int ExpiresIn);