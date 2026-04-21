using CyberZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberZone.Infrastructure.Persistence.Configurations;

public class HardwareConfiguration : IEntityTypeConfiguration<Hardware>
{
    public void Configure(EntityTypeBuilder<Hardware> builder)
    {
        builder.HasKey(h => h.Id);

        // PcNumber must be unique
        builder.Property(h => h.PcNumber)
            .IsRequired()
   .HasMaxLength(50);

        builder.HasIndex(h => new { h.ClubId, h.PcNumber })
            .IsUnique();

        builder.Property(h => h.Status)
              .HasConversion<string>()
         .HasMaxLength(20);

        // JSONB for Specs (CPU, GPU, RAM, etc.)
        builder.Property(h => h.Specs)
               .HasColumnType("nvarchar(max)")
       .HasConversion(
       v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                   v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>());

        builder.HasOne(h => h.Club)
        .WithMany(c => c.Hardwares)
                .HasForeignKey(h => h.ClubId)
      .OnDelete(DeleteBehavior.Restrict);
    }
}
