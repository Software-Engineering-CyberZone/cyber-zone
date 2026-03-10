using CyberZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberZone.Infrastructure.Persistence.Configurations;

public class GamingSessionConfiguration : IEntityTypeConfiguration<GamingSession>
{
    public void Configure(EntityTypeBuilder<GamingSession> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Status)
   .HasConversion<string>()
       .HasMaxLength(20);

        builder.Property(s => s.TotalCost)
       .HasPrecision(18, 2);

        builder.HasOne(s => s.User)
 .WithMany(u => u.GamingSessions)
     .HasForeignKey(s => s.UserId)
   .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Hardware)
   .WithMany(h => h.GamingSessions)
       .HasForeignKey(s => s.HardwareId)
     .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Tariff)
        .WithMany(t => t.GamingSessions)
            .HasForeignKey(s => s.TariffId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
