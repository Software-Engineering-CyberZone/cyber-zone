using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CyberZone.Application.DTOs;
using CyberZone.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using CyberZone.Application.DTOs;
using CyberZone.Application.Interfaces;
namespace Infrastructure.Services;

public class ClubService : IClubService
{
    private readonly IApplicationDbContext _context;

    public ClubService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ClubCatalogDto>> GetClubsForCatalogAsync()
    {
        return await _context.Clubs
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
    }
}