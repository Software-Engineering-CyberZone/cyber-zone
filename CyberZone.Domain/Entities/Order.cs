using CyberZone.Domain.Common;
using CyberZone.Domain.Enums;

namespace CyberZone.Domain.Entities;

public class Order : EntityBase, IAuditable
{
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Automatically populated from the user's active Session.Hardware.PcNumber.
    /// </summary>
    public string? PcNumber { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public ICollection<OrderItem> Items { get; set; } = [];

    public void CalculateTotal()
    {
        TotalAmount = Items.Sum(i => i.UnitPrice * i.Quantity);
    }
}
