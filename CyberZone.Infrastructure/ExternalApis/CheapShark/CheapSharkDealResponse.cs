using System.Text.Json.Serialization;

namespace CyberZone.Infrastructure.ExternalApis.CheapShark;

/// <summary>
/// Raw DTO as returned by CheapShark's /api/1.0/deals endpoint.
/// All prices come as strings — converted by our JsonNumberHandling settings.
/// </summary>
public class CheapSharkDealResponse
{
    [JsonPropertyName("internalName")]
    public string? InternalName { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("metacriticLink")]
    public string? MetacriticLink { get; set; }

    [JsonPropertyName("dealID")]
    public string? DealId { get; set; }

    [JsonPropertyName("storeID")]
    public string? StoreId { get; set; }

    [JsonPropertyName("gameID")]
    public string? GameId { get; set; }

    [JsonPropertyName("salePrice")]
    public string? SalePrice { get; set; }

    [JsonPropertyName("normalPrice")]
    public string? NormalPrice { get; set; }

    [JsonPropertyName("isOnSale")]
    public string? IsOnSale { get; set; }

    [JsonPropertyName("savings")]
    public string? Savings { get; set; }

    [JsonPropertyName("metacriticScore")]
    public string? MetacriticScore { get; set; }

    [JsonPropertyName("steamRatingText")]
    public string? SteamRatingText { get; set; }

    [JsonPropertyName("steamRatingPercent")]
    public string? SteamRatingPercent { get; set; }

    [JsonPropertyName("steamRatingCount")]
    public string? SteamRatingCount { get; set; }

    [JsonPropertyName("steamAppID")]
    public string? SteamAppId { get; set; }

    [JsonPropertyName("releaseDate")]
    public long? ReleaseDate { get; set; }

    [JsonPropertyName("lastChange")]
    public long? LastChange { get; set; }

    [JsonPropertyName("dealRating")]
    public string? DealRating { get; set; }

    [JsonPropertyName("thumb")]
    public string? Thumb { get; set; }
}
