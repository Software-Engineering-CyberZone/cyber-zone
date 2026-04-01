namespace CyberZone.Application.DTOs;

public class EditUserProfileDto
{
    public string UserId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? FullName { get; set; }
    public string? Bio { get; set; }
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? ProfileImagePath { get; set; }
}
