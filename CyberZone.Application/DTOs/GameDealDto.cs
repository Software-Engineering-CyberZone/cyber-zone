namespace CyberZone.Application.DTOs;

/// <summary>
/// Normalised game deal, surfaced to UI from CheapShark.
/// Prices are in USD as returned by the source API.
/// </summary>
public class GameDealDto
{
    public string Title { get; set; } = null!;
    public decimal SalePrice { get; set; }
    public decimal NormalPrice { get; set; }
    public decimal SavingsPercent { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? StoreName { get; set; }
    public string? SteamAppId { get; set; }
    public string? DealId { get; set; }
    public int? MetacriticScore { get; set; }

    /// <summary>Deep link to the deal on CheapShark.</summary>
    public string? DealUrl => DealId is null ? null : $"https://www.cheapshark.com/redirect?dealID={DealId}";
}
