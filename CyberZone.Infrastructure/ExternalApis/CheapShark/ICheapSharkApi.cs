using Refit;

namespace CyberZone.Infrastructure.ExternalApis.CheapShark;

/// <summary>
/// CheapShark API — generated (Refit) client.
/// Docs: https://apidocs.cheapshark.com/
/// </summary>
public interface ICheapSharkApi
{
    /// <summary>
    /// Returns active deals sorted by the given column. Default sort is "Deal Rating".
    /// </summary>
    /// <param name="pageNumber">Zero-based page index.</param>
    /// <param name="pageSize">Deals per page (max 60).</param>
    /// <param name="sortBy">Deal Rating, Title, Savings, Price, Metacritic, Reviews, Release, Store, Recent</param>
    /// <param name="storeId">Optional filter to a single storefront (Steam = "1").</param>
    /// <param name="onSale">"1" returns only items that are currently discounted.</param>
    /// <param name="aaa">"1" restricts to AAA titles (top-35 publishers per CheapShark).</param>
    /// <param name="lowerPrice">Only deals with sale price >= this value.</param>
    [Get("/api/1.0/deals")]
    Task<IReadOnlyList<CheapSharkDealResponse>> GetDealsAsync(
        [Query] int pageNumber = 0,
        [Query] int pageSize = 20,
        [Query] string sortBy = "Savings",
        [Query] string? storeID = null,
        [Query] string? onSale = "1",
        [Query] string? AAA = null,
        [Query] decimal? lowerPrice = null,
        CancellationToken ct = default);
}
