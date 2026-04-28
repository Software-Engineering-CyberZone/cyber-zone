using CyberZone.Application.Common;
using CyberZone.Application.DTOs;

namespace CyberZone.Application.Interfaces;

public interface IDealsService
{
    /// <summary>
    /// Returns a cached list of current game deals.
    /// On upstream failure, returns Result.Failure and UI can fall back gracefully.
    /// </summary>
    Task<Result<IReadOnlyList<GameDealDto>>> GetDealsAsync(int pageSize = 20, string sortBy = "Savings", CancellationToken ct = default);

    /// <summary>
    /// Recent AAA deals — newly discounted titles from top-35 publishers.
    /// Used by the homepage sidebar to highlight "fresh AAA discounts".
    /// </summary>
    Task<Result<IReadOnlyList<GameDealDto>>> GetAaaHighlightsAsync(int pageSize = 8, CancellationToken ct = default);
}
