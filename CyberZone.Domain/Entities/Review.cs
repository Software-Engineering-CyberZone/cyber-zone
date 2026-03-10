using CyberZone.Domain.Common;

namespace CyberZone.Domain.Entities;

public class Review : EntityBase, IAuditable
{
    /// <summary>
    /// Rating between 1 and 5.
    /// </summary>
    public int Rating { get; set; }
    public string? Comment { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid ClubId { get; set; }
    public Club Club { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
