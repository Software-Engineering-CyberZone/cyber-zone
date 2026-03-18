using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberZone.Application.DTOs;

public class ClubCatalogDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string FullAddress { get; set; } = null!;
    public decimal MinPrice { get; set; }
    public double Rating { get; set; }
    public string? ImageUrl { get; set; }
}