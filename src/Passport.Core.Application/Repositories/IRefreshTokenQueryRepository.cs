using Gestalt.Lib.Primitives;

namespace Passport.Core.Application.Repositories;

/// <summary>
/// Query-side repository for refresh token lookups by hash.
/// Used during the refresh flow to find the owning user.
/// </summary>
public interface IRefreshTokenQueryRepository
{
/// <summary>
/// Finds the email of the user that owns a refresh token with the given hash.
/// Returns None if the token hash is not found.
/// </summary>
Task<Option<string>> FindEmailByHashAsync(string tokenHash, CancellationToken cancellationToken);
}