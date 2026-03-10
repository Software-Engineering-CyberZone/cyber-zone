using CyberZone.Domain.Enums;

namespace CyberZone.Application.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string? PcNumber { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public List<OrderItemDto> Items { get; set; } = [];
}

public class OrderItemDto
{
    public Guid MenuItemId { get; set; }
    public string? MenuItemName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
