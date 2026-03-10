using CyberZone.Domain.Common;
using CyberZone.Domain.Enums;

namespace CyberZone.Domain.Entities;

public class GamingSession : EntityBase, IAuditable
{
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Active;
    public decimal TotalCost { get; set; }

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
    /// Ends the session and calculates the total cost based on the tariff.
    /// </summary>
    public void EndSession()
    {
        if (Status != SessionStatus.Active)
            throw new InvalidOperationException("Only active sessions can be ended.");

        EndTime = DateTime.UtcNow;
        Status = SessionStatus.Completed;

        var duration = (decimal)(EndTime.Value - StartTime).TotalHours;
        TotalCost = Math.Round(duration * Tariff.PricePerHour, 2);
    }
}
