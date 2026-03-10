using CyberZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberZone.Infrastructure.Persistence.Configurations;

/// <summary>
/// OrderItem is a junction table with a composite key (OrderId, MenuItemId).
/// UnitPrice is a snapshot at the time of purchase.
/// </summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        // Composite key
        builder.HasKey(oi => new { oi.OrderId, oi.MenuItemId });

        builder.Property(oi => oi.Quantity)
            .IsRequired()
         .HasDefaultValue(1);

        builder.Property(oi => oi.UnitPrice)
                 .HasPrecision(18, 2);

        builder.HasOne(oi => oi.Order)
        .WithMany(o => o.Items)
    .HasForeignKey(oi => oi.OrderId)
       .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oi => oi.MenuItem)
      .WithMany(m => m.OrderItems)
 .HasForeignKey(oi => oi.MenuItemId)
.OnDelete(DeleteBehavior.Restrict);
    }
}
