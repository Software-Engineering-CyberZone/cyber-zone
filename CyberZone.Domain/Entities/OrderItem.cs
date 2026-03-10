namespace CyberZone.Domain.Entities;

/// <summary>
/// Junction table with a composite key (OrderId, MenuItemId).
/// Stores a snapshot of UnitPrice at the time of purchase.
/// </summary>
public class OrderItem
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public Guid MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;

    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Price snapshot at the time of purchase — not a reference to MenuItem.Price.
    /// </summary>
    public decimal UnitPrice { get; set; }
}
