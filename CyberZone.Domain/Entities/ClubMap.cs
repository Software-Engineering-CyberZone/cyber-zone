using CyberZone.Domain.Common;

namespace CyberZone.Domain.Entities;

public class ClubMap : EntityBase, IAuditable
{
    public int Width { get; set; } = 1000;
    public int Height { get; set; } = 600;
    public string? BackgroundColor { get; set; }

    public Guid ClubId { get; set; }
    public Club Club { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public ICollection<ClubMapZone> Zones { get; set; } = [];
    public ICollection<ClubMapElement> Elements { get; set; } = [];
}
