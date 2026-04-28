using System.Globalization;
using CyberZone.Application.Common;
using CyberZone.Application.DTOs;
using CyberZone.Application.Interfaces;
using CyberZone.Infrastructure.ExternalApis.CheapShark;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CyberZone.Infrastructure.Services;

public class DealsService : IDealsService
{
    private static readonly IReadOnlyDictionary<string, string> StoreNames = new Dictionary<string, string>
    {
        ["1"] = "Steam",
        ["2"] = "GamersGate",
        ["3"] = "GreenManGaming",
        ["7"] = "GOG",
        ["8"] = "Origin",
        ["11"] = "Humble Store",
        ["13"] = "Uplay",
        ["15"] = "Fanatical",
        ["21"] = "WinGameStore",
        ["23"] = "GameBillet",
        ["25"] = "Epic Games Store",
        ["27"] = "GameSplanet",
        ["28"] = "2Game",
        ["29"] = "IndieGala",
        ["30"] = "Blizzard Shop",
        ["31"] = "AllYouPlay",
        ["32"] = "DLGamer",
        ["33"] = "Noctre",
        ["34"] = "DreamGame"
    };

    private readonly ICheapSharkApi _api;
    private readonly ICacheService _cache;
    private readonly CacheOptions _cacheOptions;
    private readonly ILogger<DealsService> _logger;

    public DealsService(
        ICheapSharkApi api,
        ICacheService cache,
        IOptions<CacheOptions> cacheOptions,
        ILogger<DealsService> logger)
    {
        _api = api;
        _cache = cache;
        _cacheOptions = cacheOptions.Value;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<GameDealDto>>> GetDealsAsync(int pageSize = 20, string sortBy = "Savings", CancellationToken ct = default)
    {
        var cached = await _cache.GetOrSetAsync(
            CacheKeys.GameDeals(pageSize, sortBy),
            () => LoadAsync(pageSize, sortBy, aaaOnly: false, ct),
            TimeSpan.FromMinutes(_cacheOptions.GameDealsMinutes));

        return cached is null
            ? Result.Failure<IReadOnlyList<GameDealDto>>("Знижки тимчасово недоступні, спробуйте пізніше.")
            : Result.Success<IReadOnlyList<GameDealDto>>(cached);
    }

    public async Task<Result<IReadOnlyList<GameDealDto>>> GetAaaHighlightsAsync(int pageSize = 8, CancellationToken ct = default)
    {
        var cached = await _cache.GetOrSetAsync(
            CacheKeys.GameAaaHighlights(pageSize),
            () => LoadAsync(pageSize, sortBy: "Recent", aaaOnly: true, ct),
            TimeSpan.FromMinutes(_cacheOptions.GameDealsMinutes));

        return cached is null
            ? Result.Failure<IReadOnlyList<GameDealDto>>("Новинки тимчасово недоступні.")
            : Result.Success<IReadOnlyList<GameDealDto>>(cached);
    }

    private async Task<List<GameDealDto>?> LoadAsync(int pageSize, string sortBy, bool aaaOnly, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Fetching {PageSize} deals from CheapShark (sort={SortBy}, aaa={Aaa})", pageSize, sortBy, aaaOnly);

            var deals = await _api.GetDealsAsync(
                pageSize: pageSize,
                sortBy: sortBy,
                AAA: aaaOnly ? "1" : null,
                ct: ct);

            return deals
                .Where(d => !string.IsNullOrWhiteSpace(d.Title))
                .Select(Map)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch deals from CheapShark");
            return null;
        }
    }

    private static GameDealDto Map(CheapSharkDealResponse r) => new()
    {
        Title = r.Title!,
        SalePrice = ParseDecimal(r.SalePrice),
        NormalPrice = ParseDecimal(r.NormalPrice),
        SavingsPercent = Math.Round(ParseDecimal(r.Savings), 0),
        ThumbnailUrl = r.Thumb,
        DealId = r.DealId,
        SteamAppId = r.SteamAppId,
        StoreName = r.StoreId is not null && StoreNames.TryGetValue(r.StoreId, out var name) ? name : $"Store {r.StoreId}",
        MetacriticScore = int.TryParse(r.MetacriticScore, out var score) && score > 0 ? score : null
    };

    private static decimal ParseDecimal(string? s) =>
        decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0m;
}
