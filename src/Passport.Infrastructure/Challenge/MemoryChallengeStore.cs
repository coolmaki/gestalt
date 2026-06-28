using System.Collections.Concurrent;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Services;

namespace Passport.Infrastructure.Challenge;

internal sealed class MemoryChallengeStore : IChallengeStore
{
    private readonly ConcurrentDictionary<string, (byte[] Challenge, DateTimeOffset ExpiresAt)> _store = new();

    public Task SetAsync(string key, byte[] challenge, TimeSpan ttl, CancellationToken cancellationToken)
    {
        _store[key] = (challenge, DateTimeOffset.UtcNow + ttl);
        return Task.CompletedTask;
    }

    public Task<Option<byte[]>> GetAndRemoveAsync(string key, CancellationToken cancellationToken)
    {
        if (_store.TryRemove(key, out var entry))
        {
            if (DateTimeOffset.UtcNow < entry.ExpiresAt)
            {
                return Task.FromResult(Option<byte[]>.Some(entry.Challenge));
            }
        }

        // Clean up expired entries lazily
        var expiredKeys = _store.Where(kvp => DateTimeOffset.UtcNow >= kvp.Value.ExpiresAt).Select(kvp => kvp.Key).ToList();
        foreach (var expiredKey in expiredKeys)
        {
            _store.TryRemove(expiredKey, out _);
        }

        return Task.FromResult(Option<byte[]>.None);
    }
}