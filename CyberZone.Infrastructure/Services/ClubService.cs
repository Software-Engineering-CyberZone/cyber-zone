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
            .Include(c => c.Reviews)
            .Select(c => new ClubCatalogDto
            {
                Id = c.Id,
                Name = c.Name,
                FullAddress = $"{c.Address.City}, {c.Address.Street}, {c.Address.ZipCode}",
                Rating = c.Reviews.Any() ? c.Reviews.Average(r => (double)r.Rating) : 0,
                ReviewCount = c.Reviews.Count,
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
            .Include(c => c.Reviews).ThenInclude(r => r.User)
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
            Description = club.Description,
            FullAddress = $"{club.Address.City}, {club.Address.Street}, {club.Address.ZipCode}",
            Phone = club.Phone,
            Email = club.Email,
            Rating = club.Reviews.Any() ? club.Reviews.Average(r => (double)r.Rating) : 0,
            ReviewCount = club.Reviews.Count,
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
            }).ToList(),
            Reviews = club.Reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                UserName = r.User?.UserName ?? "Користувач",
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).OrderByDescending(r => r.CreatedAt).ToList()
        };

        _logger.LogInformation("Fetched club details for {ClubName} ({ClubId})", club.Name, club.Id);
        return Result.Success(dto);
    }

    public async Task<Result<EditClubDto>> GetClubForEditAsync(Guid id)
    {
        _logger.LogInformation("Fetching club details for edit {ClubId}", id);

        var club = await _context.Clubs.FirstOrDefaultAsync(c => c.Id == id);
        
        if (club is null)
        {
            _logger.LogWarning("Club {ClubId} not found", id);
            return Result.Failure<EditClubDto>($"Club with ID '{id}' was not found.");
        }

        var editDto = new EditClubDto
        {
            Id = club.Id,
            Name = club.Name,
            Description = club.Description,
            Phone = club.Phone,
            Email = club.Email,
            Street = club.Address.Street,
            City = club.Address.City,
            State = club.Address.State,
            ZipCode = club.Address.ZipCode,
            Country = club.Address.Country,
            WorkHours = club.WorkHours
        };

        return Result.Success(editDto);
    }

    public async Task<Result<bool>> UpdateClubAsync(Guid id, EditClubDto dto)
    {
        _logger.LogInformation("Updating club details for {ClubId}", id);

        var club = await _context.Clubs.FirstOrDefaultAsync(c => c.Id == id);
        
        if (club is null)
        {
            _logger.LogWarning("Club {ClubId} not found for update", id);
            return Result.Failure<bool>($"Club with ID '{id}' was not found.");
        }

        club.Name = dto.Name;
        club.Description = dto.Description;
        club.Phone = dto.Phone;
        club.Email = dto.Email;
        
        club.Address = new CyberZone.Domain.ValueObjects.Address
        {
            Street = dto.Street,
            City = dto.City,
            State = dto.State,
            ZipCode = dto.ZipCode,
            Country = dto.Country
        };

        if (dto.WorkHours != null)
        {
            club.WorkHours = dto.WorkHours;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Successfully updated club details for {ClubId}", id);

        return Result.Success(true);
    }

    public async Task<Result<bool>> AddTariffAsync(CreateTariffDto dto)
    {
        var club = await _context.Clubs.FindAsync(dto.ClubId);
        if (club == null) return Result.Failure<bool>("Club not found");

        var tariff = new CyberZone.Domain.Entities.Tariff
        {
            Name = dto.Name,
            Type = dto.Type,
            PricePerHour = dto.PricePerHour,
            Description = dto.Description,
            ClubId = dto.ClubId
        };

        _context.Tariffs.Add(tariff);
        await _context.SaveChangesAsync();

        return Result.Success(true);
    }

    public async Task<Result<EditTariffDto>> GetTariffForEditAsync(Guid id)
    {
        var tariff = await _context.Tariffs.FindAsync(id);
        if (tariff == null) return Result.Failure<EditTariffDto>("Tariff not found");

        var dto = new EditTariffDto
        {
            Id = tariff.Id,
            ClubId = tariff.ClubId,
            Name = tariff.Name,
            Type = tariff.Type,
            PricePerHour = tariff.PricePerHour,
            Description = tariff.Description
        };

        return Result.Success(dto);
    }

    public async Task<Result<bool>> UpdateTariffAsync(Guid id, EditTariffDto dto)
    {
        var tariff = await _context.Tariffs.FindAsync(id);
        if (tariff == null) return Result.Failure<bool>("Tariff not found");

        tariff.Name = dto.Name;
        tariff.Type = dto.Type;
        tariff.PricePerHour = dto.PricePerHour;
        tariff.Description = dto.Description;

        await _context.SaveChangesAsync();
        return Result.Success(true);
    }

    public async Task<Result<bool>> DeleteTariffAsync(Guid id)
    {
        var tariff = await _context.Tariffs.FindAsync(id);
        if (tariff == null) return Result.Failure<bool>("Tariff not found");

        _context.Tariffs.Remove(tariff);
        await _context.SaveChangesAsync();
        
        return Result.Success(true);
    }
}
