using CyberZone.Domain.Common;
using CyberZone.Domain.Enums;

namespace CyberZone.Domain.Entities;

public class Tariff : EntityBase, IAuditable
{
    public string Name { get; set; } = string.Empty;
    public TariffType Type { get; set; }
    public decimal PricePerHour { get; set; }
    public string? Description { get; set; }

    public Guid ClubId { get; set; }
    public Club Club { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public ICollection<Booking> Bookings { get; set; } = [];
    public ICollection<GamingSession> GamingSessions { get; set; } = [];
}
