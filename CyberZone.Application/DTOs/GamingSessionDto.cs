using CyberZone.Domain.Enums;

namespace CyberZone.Application.DTOs;

public class GamingSessionDto
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public SessionStatus Status { get; set; }
    public decimal TotalCost { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public Guid HardwareId { get; set; }
    public string? PcNumber { get; set; }
    public string? TariffName { get; set; }
}
