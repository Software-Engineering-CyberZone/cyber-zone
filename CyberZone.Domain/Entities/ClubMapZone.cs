using CyberZone.Domain.Common;

namespace CyberZone.Domain.Entities;

public class ClubMapZone : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string? LabelColor { get; set; }
    public string? BorderColor { get; set; }

    public Guid ClubMapId { get; set; }
    public ClubMap ClubMap { get; set; } = null!;

    public ICollection<ClubMapElement> Elements { get; set; } = [];
}
