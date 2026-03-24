using CyberZone.Application.Common;
using CyberZone.Application.DTOs;
using CyberZone.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class ClubService : IClubService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ClubService> _logger;

    public ClubService(IApplicationDbContext context, ILogger<ClubService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ClubCatalogDto>>> GetClubsForCatalogAsync()
    {
        _logger.LogInformation("Fetching clubs for catalog");

        var clubs = await _context.Clubs
            .Include(c => c.Tariffs)
            .Select(c => new ClubCatalogDto
            {
                Id = c.Id,
                Name = c.Name,
                FullAddress = $"{c.Address.City}, {c.Address.Street}, {c.Address.ZipCode}",
                Rating = c.Rating,
                ImageUrl = "/images/welcome_gaming.png",
                MinPrice = c.Tariffs.Any() ? c.Tariffs.Min(t => t.PricePerHour) : 0
            })
            .ToListAsync();

        _logger.LogInformation("Fetched {Count} clubs for catalog", clubs.Count);
        return Result.Success<IEnumerable<ClubCatalogDto>>(clubs);
    }

    public async Task<Result<ClubDetailsDto>> GetClubDetailsAsync(Guid id)
    {
        _logger.LogInformation("Fetching club details for {ClubId}", id);

        var club = await _context.Clubs
            .Include(c => c.Hardwares)
            .Include(c => c.Tariffs)
            .Include(c => c.MenuItems)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (club is null)
        {
            _logger.LogWarning("Club {ClubId} not found", id);
            return Result.Failure<ClubDetailsDto>($"Club with ID '{id}' was not found.");
        }

        var dto = new ClubDetailsDto
        {
            Id = club.Id,
            Name = club.Name,
            FullAddress = $"{club.Address.City}, {club.Address.Street}, {club.Address.ZipCode}",
            Phone = club.Phone,
            Email = club.Email,
            Rating = club.Rating,
            WorkHours = club.WorkHours,
            Hardwares = club.Hardwares.Select(h => new HardwareDto
            {
                Id = h.Id,
                PcNumber = h.PcNumber,
                Status = h.Status,
                Specs = h.Specs
            }).ToList(),
            Tariffs = club.Tariffs.Select(t => new TariffDto
            {
                Id = t.Id,
                Name = t.Name,
                Type = t.Type,
                PricePerHour = t.PricePerHour,
                Description = t.Description
            }).ToList(),
            MenuItems = club.MenuItems.Select(m => new MenuItemDto
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                Category = m.Category,
                IsAvailable = m.IsAvailable
            }).ToList()
        };

        _logger.LogInformation("Fetched club details for {ClubName} ({ClubId})", club.Name, club.Id);
        return Result.Success(dto);
    }
}
