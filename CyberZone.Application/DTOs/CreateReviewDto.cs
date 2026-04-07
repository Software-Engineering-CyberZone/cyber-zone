using System.ComponentModel.DataAnnotations;

namespace CyberZone.Application.DTOs;

public class CreateReviewDto
{
    [Required]
    public Guid ClubId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }
}
