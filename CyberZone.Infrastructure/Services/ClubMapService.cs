using CyberZone.Application.Common;
using CyberZone.Application.DTOs;
using CyberZone.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CyberZone.Infrastructure.Services;

public class ClubMapService : IClubMapService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ClubMapService> _logger;
    private readonly ICacheService _cache;
    private readonly CacheOptions _cacheOptions;

    public ClubMapService(
        IApplicationDbContext context,
        ILogger<ClubMapService> logger,
        ICacheService cache,
        IOptions<CacheOptions> cacheOptions)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
        _cacheOptions = cacheOptions.Value;
    }

    public async Task<Result<ClubMapDto>> GetMapByClubIdAsync(Guid clubId)
    {
        var cached = await _cache.GetOrSetAsync(
            CacheKeys.ClubMap(clubId),
            () => LoadMapAsync(clubId),
            TimeSpan.FromMinutes(_cacheOptions.ClubMapMinutes));

        return cached is null
            ? Result.Failure<ClubMapDto>($"Map for club '{clubId}' was not found.")
            : Result.Success(cached);
    }

    private async Task<ClubMapDto?> LoadMapAsync(Guid clubId)
    {
        _logger.LogInformation("Fetching club map for {ClubId}", clubId);

        var map = await _context.ClubMaps
            .AsNoTracking()
            .Include(m => m.Club)
            .Include(m => m.Zones)
            .Include(m => m.Elements)
                .ThenInclude(e => e.Hardware)
            .FirstOrDefaultAsync(m => m.ClubId == clubId);

        if (map is null)
        {
            _logger.LogWarning("Club map not found for {ClubId}", clubId);
            return null;
        }

        var minPrice = await _context.Tariffs
            .Where(t => t.ClubId == clubId)
            .Select(t => (decimal?)t.PricePerHour)
            .OrderBy(p => p)
            .FirstOrDefaultAsync();

        return new ClubMapDto
        {
            Id = map.Id,
            ClubId = map.ClubId,
            ClubName = map.Club.Name,
            Width = map.Width,
            Height = map.Height,
            BackgroundColor = map.BackgroundColor,
            Zones = map.Zones.Select(z => new ClubMapZoneDto
            {
                Id = z.Id,
                Name = z.Name,
                X = z.X,
                Y = z.Y,
                Width = z.Width,
                Height = z.Height,
                LabelColor = z.LabelColor,
                BorderColor = z.BorderColor
            }).ToList(),
            Elements = map.Elements.Select(e => new ClubMapElementDto
            {
                Id = e.Id,
                ElementType = e.ElementType,
                X = e.X,
                Y = e.Y,
                Width = e.Width,
                Height = e.Height,
                Rotation = e.Rotation,
                Label = e.Label,
                ZoneId = e.ZoneId,
                HardwareId = e.HardwareId,
                PcNumber = e.Hardware?.PcNumber,
                HardwareStatus = e.Hardware?.Status,
                MinPricePerHour = minPrice
            }).ToList()
        };
    }
}
