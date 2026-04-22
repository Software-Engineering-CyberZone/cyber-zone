using System.Collections.Concurrent;
using CyberZone.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CyberZone.Infrastructure.Services;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;
    private readonly ConcurrentDictionary<string, byte> _keys = new();

    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan ttl) where T : class
    {
        if (_cache.TryGetValue(key, out T? cached))
        {
            _logger.LogDebug("Cache HIT: {Key}", key);
            return cached;
        }

        _logger.LogDebug("Cache MISS: {Key}", key);
        var value = await factory();
        if (value is not null)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };
            options.RegisterPostEvictionCallback((k, _, _, _) => _keys.TryRemove(k.ToString()!, out _));

            _cache.Set(key, value, options);
            _keys.TryAdd(key, 0);
        }
        return value;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        _keys.TryRemove(key, out _);
        _logger.LogDebug("Cache INVALIDATE: {Key}", key);
    }

    public void RemoveByPrefix(string prefix)
    {
        foreach (var key in _keys.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).ToList())
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
        }
        _logger.LogDebug("Cache INVALIDATE by prefix: {Prefix}", prefix);
    }
}
