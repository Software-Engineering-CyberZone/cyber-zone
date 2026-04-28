namespace CyberZone.Application.Common;

public class CacheOptions
{
    public const string SectionName = "Cache";

    public int ClubCatalogMinutes { get; set; } = 10;
    public int ClubDetailsMinutes { get; set; } = 5;
    public int ClubMapMinutes { get; set; } = 2;
    public int GameDealsMinutes { get; set; } = 30;
}
