using CyberZone.Application.Common;
using CyberZone.Application.DTOs;

namespace CyberZone.Application.Interfaces;

public interface IClubService
{
    Task<Result<IEnumerable<ClubCatalogDto>>> GetClubsForCatalogAsync();
    Task<Result<ClubDetailsDto>> GetClubDetailsAsync(Guid id);
    Task<Result<EditClubDto>> GetClubForEditAsync(Guid id);
    Task<Result<bool>> UpdateClubAsync(Guid id, EditClubDto dto);

    Task<Result<bool>> AddTariffAsync(CreateTariffDto dto);
    Task<Result<EditTariffDto>> GetTariffForEditAsync(Guid id);
    Task<Result<bool>> UpdateTariffAsync(Guid id, EditTariffDto dto);
    Task<Result<bool>> DeleteTariffAsync(Guid id);
}
