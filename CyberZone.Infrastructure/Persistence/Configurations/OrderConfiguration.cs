using CyberZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberZone.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Status)
         .HasConversion<string>()
    .HasMaxLength(20);

        builder.Property(o => o.TotalAmount)
      .HasPrecision(18, 2);

        builder.Property(o => o.PcNumber)
            .HasMaxLength(50);

        builder.HasOne(o => o.User)
           .WithMany(u => u.Orders)
             .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
