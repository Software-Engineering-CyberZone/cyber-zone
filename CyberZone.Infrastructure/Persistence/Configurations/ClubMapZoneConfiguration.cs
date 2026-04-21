using CyberZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberZone.Infrastructure.Persistence.Configurations;

public class ClubMapZoneConfiguration : IEntityTypeConfiguration<ClubMapZone>
{
    public void Configure(EntityTypeBuilder<ClubMapZone> builder)
    {
        builder.HasKey(z => z.Id);

        builder.Property(z => z.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(z => z.LabelColor).HasMaxLength(32);
        builder.Property(z => z.BorderColor).HasMaxLength(32);
    }
}
