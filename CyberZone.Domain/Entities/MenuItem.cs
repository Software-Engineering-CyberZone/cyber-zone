using CyberZone.Domain.Common;

namespace CyberZone.Domain.Entities;

public class MenuItem : EntityBase, IAuditable
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Category { get; set; }
    public bool IsAvailable { get; set; } = true;

    public Guid ClubId { get; set; }
    public Club Club { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}
