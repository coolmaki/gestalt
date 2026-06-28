using Supercluster.Lib.Primitives;

namespace Passport.Core.Application.Services;

/// <summary>
/// Temporary storage for WebAuthn challenges between begin/complete calls.
/// In production, backed by IMemoryCache or Redis.
/// </summary>
public interface IChallengeStore
{
    /// <summary>
    /// Stores a challenge with a time-to-live.
    /// </summary>
    Task SetAsync(string key, byte[] challenge, TimeSpan ttl, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves and removes a challenge. Returns <see cref="Option{T}.None"/> if not found or expired.
    /// </summary>
    Task<Option<byte[]>> GetAndRemoveAsync(string key, CancellationToken cancellationToken);
}