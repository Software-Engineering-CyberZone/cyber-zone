using CyberZone.Domain.Enums;

namespace CyberZone.Application.DTOs;

public class BookingDto
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public BookingStatus Status { get; set; }
    public string? Notes { get; set; }
  public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public Guid HardwareId { get; set; }
    public string? PcNumber { get; set; }
    public Guid TariffId { get; set; }
    public string? TariffName { get; set; }
    public decimal PricePerHour { get; set; }
}
