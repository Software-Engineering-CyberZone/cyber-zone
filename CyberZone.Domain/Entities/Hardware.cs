using CyberZone.Domain.Common;
using CyberZone.Domain.Enums;

namespace CyberZone.Domain.Entities;

public class Hardware : EntityBase, IAuditable
{
    /// <summary>
    /// Unique PC identifier within the club (e.g., "PC-01").
    /// </summary>
    public string PcNumber { get; set; } = string.Empty;

    public HardwareStatus Status { get; set; } = HardwareStatus.Available;

    /// <summary>
    /// JSONB column storing hardware specifications (e.g., CPU, GPU, RAM).
    /// </summary>
    public Dictionary<string, string> Specs { get; set; } = [];

    public Guid ClubId { get; set; }
    public Club Club { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public ICollection<Booking> Bookings { get; set; } = [];
    public ICollection<GamingSession> GamingSessions { get; set; } = [];
    public ClubMapElement? MapElement { get; set; }
}
