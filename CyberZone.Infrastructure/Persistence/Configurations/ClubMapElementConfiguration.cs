using CyberZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberZone.Infrastructure.Persistence.Configurations;

public class ClubMapElementConfiguration : IEntityTypeConfiguration<ClubMapElement>
{
    public void Configure(EntityTypeBuilder<ClubMapElement> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ElementType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.Label).HasMaxLength(50);

        builder.HasOne(e => e.Zone)
            .WithMany(z => z.Elements)
            .HasForeignKey(e => e.ZoneId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Hardware)
            .WithOne(h => h.MapElement!)
            .HasForeignKey<ClubMapElement>(e => e.HardwareId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
