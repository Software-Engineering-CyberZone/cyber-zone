using CyberZone.Domain.Enums;

namespace CyberZone.Application.DTOs;

public class ClubDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string FullAddress { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public Dictionary<string, string> WorkHours { get; set; } = [];
    public List<HardwareDto> Hardwares { get; set; } = [];
    public List<TariffDto> Tariffs { get; set; } = [];
    public List<MenuItemDto> MenuItems { get; set; } = [];
    public List<ReviewDto> Reviews { get; set; } = [];
}

public class HardwareDto
{
    public Guid Id { get; set; }
    public string PcNumber { get; set; } = null!;
    public HardwareStatus Status { get; set; }
    public Dictionary<string, string> Specs { get; set; } = [];
}

public class TariffDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public TariffType Type { get; set; }
    public decimal PricePerHour { get; set; }
    public string? Description { get; set; }
}

public class MenuItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Category { get; set; }
    public bool IsAvailable { get; set; }
}

public class ReviewDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
