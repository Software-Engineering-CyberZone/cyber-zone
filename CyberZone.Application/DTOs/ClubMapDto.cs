using CyberZone.Domain.Enums;

namespace CyberZone.Application.DTOs;

public class ClubMapDto
{
    public Guid Id { get; set; }
    public Guid ClubId { get; set; }
    public string ClubName { get; set; } = null!;
    public int Width { get; set; }
    public int Height { get; set; }
    public string? BackgroundColor { get; set; }
    public List<ClubMapZoneDto> Zones { get; set; } = [];
    public List<ClubMapElementDto> Elements { get; set; } = [];
}

public class ClubMapZoneDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string? LabelColor { get; set; }
    public string? BorderColor { get; set; }
}

public class ClubMapElementDto
{
    public Guid Id { get; set; }
    public ClubMapElementType ElementType { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Rotation { get; set; }
    public string? Label { get; set; }
    public Guid? ZoneId { get; set; }

    public Guid? HardwareId { get; set; }
    public string? PcNumber { get; set; }
    public HardwareStatus? HardwareStatus { get; set; }
    public decimal? MinPricePerHour { get; set; }
}
