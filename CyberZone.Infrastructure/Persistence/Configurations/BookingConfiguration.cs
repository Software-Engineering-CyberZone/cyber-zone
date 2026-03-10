using CyberZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberZone.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Status)
    .HasConversion<string>()
    .HasMaxLength(20);

        builder.Property(b => b.Notes)
            .HasMaxLength(1000);

        builder.HasOne(b => b.User)
                 .WithMany(u => u.Bookings)
           .HasForeignKey(b => b.UserId)
          .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Hardware)
    .WithMany(h => h.Bookings)
            .HasForeignKey(b => b.HardwareId)
        .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Tariff)
            .WithMany(t => t.Bookings)
      .HasForeignKey(b => b.TariffId)
    .OnDelete(DeleteBehavior.Restrict);
    }
}
