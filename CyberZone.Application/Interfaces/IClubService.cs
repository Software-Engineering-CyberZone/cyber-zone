using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CyberZone.Application.DTOs;
using CyberZone.Application.Interfaces;
namespace CyberZone.Application.Interfaces;

public interface IClubService
{
    Task<IEnumerable<ClubCatalogDto>> GetClubsForCatalogAsync();
}