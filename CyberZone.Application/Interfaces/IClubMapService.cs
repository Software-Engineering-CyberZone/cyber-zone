using CyberZone.Application.Common;
using CyberZone.Application.DTOs;

namespace CyberZone.Application.Interfaces;

public interface IClubMapService
{
    Task<Result<ClubMapDto>> GetMapByClubIdAsync(Guid clubId);
}
