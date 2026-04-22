using CyberZone.Application.Common;
using CyberZone.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace CyberZone.Tests.Helpers;

/// <summary>
/// Cache that always misses — so service calls hit the underlying data source.
/// Used in unit tests where we want to assert real query behaviour.
/// </summary>
public class NoOpCacheService : ICacheService
{
    public Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan ttl) where T : class
        => factory();

    public void Remove(string key) { }
    public void RemoveByPrefix(string prefix) { }
}

public static class CacheTestHelper
{
    public static IOptions<CacheOptions> DefaultOptions() =>
        Options.Create(new CacheOptions());
}
