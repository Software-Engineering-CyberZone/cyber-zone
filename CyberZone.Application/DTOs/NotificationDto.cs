namespace CyberZone.Application.DTOs;

public class NotificationDto
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Level { get; set; } = "info";
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
