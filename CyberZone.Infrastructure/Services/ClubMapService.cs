using CyberZone.Application.Common;
using CyberZone.Application.DTOs;
using CyberZone.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberZone.Infrastructure.Services;

public class ClubMapService : IClubMapService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ClubMapService> _logger;

    public ClubMapService(IApplicationDbContext context, ILogger<ClubMapService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<ClubMapDto>> GetMapByClubIdAsync(Guid clubId)
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
            return Result.Failure<ClubMapDto>($"Map for club '{clubId}' was not found.");
        }

        var minPrice = await _context.Tariffs
            .Where(t => t.ClubId == clubId)
            .Select(t => (decimal?)t.PricePerHour)
            .OrderBy(p => p)
            .FirstOrDefaultAsync();

        var dto = new ClubMapDto
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

        return Result.Success(dto);
    }
}
