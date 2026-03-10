namespace CyberZone.Application.DTOs;

public class CreateBookingDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Notes { get; set; }
    public Guid UserId { get; set; }
    public Guid HardwareId { get; set; }
  public Guid TariffId { get; set; }
}
