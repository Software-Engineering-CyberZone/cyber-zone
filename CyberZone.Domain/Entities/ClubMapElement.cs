using CyberZone.Domain.Common;
using CyberZone.Domain.Enums;

namespace CyberZone.Domain.Entities;

public class ClubMapElement : EntityBase
{
    public ClubMapElementType ElementType { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; } = 40;
    public int Height { get; set; } = 40;
    public int Rotation { get; set; }

    /// <summary>
    /// Optional caption — e.g. PC number "1", zone marker, etc.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// For PC/Console elements — links to the physical hardware whose status drives availability.
    /// </summary>
    public Guid? HardwareId { get; set; }
    public Hardware? Hardware { get; set; }

    public Guid ClubMapId { get; set; }
    public ClubMap ClubMap { get; set; } = null!;

    public Guid? ZoneId { get; set; }
    public ClubMapZone? Zone { get; set; }
}
