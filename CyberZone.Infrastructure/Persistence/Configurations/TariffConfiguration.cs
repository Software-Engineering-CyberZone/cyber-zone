using CyberZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberZone.Infrastructure.Persistence.Configurations;

public class TariffConfiguration : IEntityTypeConfiguration<Tariff>
{
    public void Configure(EntityTypeBuilder<Tariff> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
        .HasMaxLength(100);

        builder.Property(t => t.Type)
        .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.PricePerHour)
               .HasPrecision(18, 2);

        builder.Property(t => t.Description)
 .HasMaxLength(500);

        builder.HasOne(t => t.Club)
   .WithMany(c => c.Tariffs)
            .HasForeignKey(t => t.ClubId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
