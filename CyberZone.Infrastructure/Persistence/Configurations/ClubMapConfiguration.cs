using CyberZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberZone.Infrastructure.Persistence.Configurations;

public class ClubMapConfiguration : IEntityTypeConfiguration<ClubMap>
{
    public void Configure(EntityTypeBuilder<ClubMap> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.BackgroundColor).HasMaxLength(32);

        builder.HasOne(m => m.Club)
            .WithOne(c => c.Map!)
            .HasForeignKey<ClubMap>(m => m.ClubId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.ClubId).IsUnique();

        builder.HasMany(m => m.Zones)
            .WithOne(z => z.ClubMap)
            .HasForeignKey(z => z.ClubMapId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Elements)
            .WithOne(e => e.ClubMap)
            .HasForeignKey(e => e.ClubMapId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
