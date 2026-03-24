using CyberZone.Application.Common;
using CyberZone.Application.DTOs;

namespace CyberZone.Application.Interfaces;

public interface IClubService
{
    Task<Result<IEnumerable<ClubCatalogDto>>> GetClubsForCatalogAsync();
    Task<Result<ClubDetailsDto>> GetClubDetailsAsync(Guid id);
}
