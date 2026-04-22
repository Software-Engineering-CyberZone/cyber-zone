namespace CyberZone.Application.Interfaces;

/// <summary>
/// Абстракція над IMemoryCache. Дозволяє легко замінити на IDistributedCache в майбутньому
/// і централізує схему ключів + інвалідацію.
/// </summary>
public interface ICacheService
{
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan ttl) where T : class;
    void Remove(string key);
    void RemoveByPrefix(string prefix);
}

public static class CacheKeys
{
    public const string ClubCatalog = "club:catalog";
    public const string ClubDetailsPrefix = "club:details:";
    public const string ClubMapPrefix = "club:map:";

    public static string ClubDetails(Guid id) => $"{ClubDetailsPrefix}{id}";
    public static string ClubMap(Guid clubId) => $"{ClubMapPrefix}{clubId}";
}
