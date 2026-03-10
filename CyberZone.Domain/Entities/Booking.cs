using CyberZone.Domain.Common;
using CyberZone.Domain.Enums;

namespace CyberZone.Domain.Entities;

public class Booking : EntityBase, IAuditable
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public string? Notes { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid HardwareId { get; set; }
    public Hardware Hardware { get; set; } = null!;

    public Guid TariffId { get; set; }
    public Tariff Tariff { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Transitions a confirmed Booking into an active GamingSession.
    /// </summary>
    public GamingSession TransitionToSession()
    {
        if (Status != BookingStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed bookings can be transitioned to a session.");

        Status = BookingStatus.Active;

        return new GamingSession
        {
            UserId = UserId,
            HardwareId = HardwareId,
            TariffId = TariffId,
            StartTime = DateTime.UtcNow,
            Status = SessionStatus.Active
        };
    }
}
