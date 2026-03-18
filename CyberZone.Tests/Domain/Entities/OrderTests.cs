using CyberZone.Domain.Entities;
using CyberZone.Domain.Enums;
using FluentAssertions;

namespace CyberZone.Tests.Domain.Entities;

public class OrderTests
{
    [Fact]
    public void CalculateTotal_WithMultipleItems_ReturnsSumOfAllItems()
    {
        var order = new Order
        {
            UserId = Guid.NewGuid(),
            Items = new List<OrderItem>
            {
                new() { MenuItemId = Guid.NewGuid(), UnitPrice = 10m, Quantity = 2 },
                new() { MenuItemId = Guid.NewGuid(), UnitPrice = 25m, Quantity = 1 },
                new() { MenuItemId = Guid.NewGuid(), UnitPrice = 5m, Quantity = 3 }
            }
        };

        order.CalculateTotal();

        // (10*2) + (25*1) + (5*3) = 20 + 25 + 15 = 60
        order.TotalAmount.Should().Be(60m);
    }

    [Fact]
    public void CalculateTotal_WithNoItems_ReturnsZero()
    {
        var order = new Order
        {
            UserId = Guid.NewGuid(),
            Items = new List<OrderItem>()
        };

        order.CalculateTotal();

        order.TotalAmount.Should().Be(0m);
    }

    [Fact]
    public void CalculateTotal_WithSingleItem_ReturnsCorrectTotal()
    {
        var order = new Order
        {
            UserId = Guid.NewGuid(),
            Items = new List<OrderItem>
            {
                new() { MenuItemId = Guid.NewGuid(), UnitPrice = 99.99m, Quantity = 1 }
            }
        };

        order.CalculateTotal();

        order.TotalAmount.Should().Be(99.99m);
    }

    [Fact]
    public void CalculateTotal_OverwritesPreviousTotal()
    {
        var order = new Order
        {
            UserId = Guid.NewGuid(),
            TotalAmount = 999m,
            Items = new List<OrderItem>
            {
                new() { MenuItemId = Guid.NewGuid(), UnitPrice = 10m, Quantity = 1 }
            }
        };

        order.CalculateTotal();

        order.TotalAmount.Should().Be(10m);
    }

    [Fact]
    public void DefaultStatus_IsPending()
    {
        var order = new Order();

        order.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void CalculateTotal_WithHighQuantity_CalculatesCorrectly()
    {
        var order = new Order
        {
            UserId = Guid.NewGuid(),
            Items = new List<OrderItem>
            {
                new() { MenuItemId = Guid.NewGuid(), UnitPrice = 0.50m, Quantity = 100 }
            }
        };

        order.CalculateTotal();

        order.TotalAmount.Should().Be(50m);
    }
}
