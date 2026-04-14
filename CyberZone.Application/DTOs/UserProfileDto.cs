namespace CyberZone.Application.DTOs;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? FullName { get; set; }
    public decimal Balance { get; set; }
    public string? Bio { get; set; }
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? ProfileImagePath { get; set; }
    public List<TransactionDto> Transactions { get; set; } = new();
    public List<UserReviewDto> Reviews { get; set; } = new();
}

public class UserReviewDto
{
    public Guid ClubId { get; set; }
    public string ClubName { get; set; } = null!;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}