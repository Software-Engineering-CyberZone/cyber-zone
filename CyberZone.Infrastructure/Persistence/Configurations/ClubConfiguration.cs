using CyberZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberZone.Infrastructure.Persistence.Configurations;

public class ClubConfiguration : IEntityTypeConfiguration<Club>
{
    public void Configure(EntityTypeBuilder<Club> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
         .IsRequired()
             .HasMaxLength(200);

        builder.HasIndex(c => c.Name);

        builder.Property(c => c.Phone)
.HasMaxLength(20);

        builder.Property(c => c.Email)
                .HasMaxLength(256);

        // Address value object — owned type
        builder.OwnsOne(c => c.Address, a =>
      {
          a.Property(x => x.Street).HasMaxLength(300).HasColumnName("Address_Street");
          a.Property(x => x.City).HasMaxLength(100).HasColumnName("Address_City");
          a.Property(x => x.State).HasMaxLength(100).HasColumnName("Address_State");
          a.Property(x => x.ZipCode).HasMaxLength(20).HasColumnName("Address_ZipCode");
          a.Property(x => x.Country).HasMaxLength(100).HasColumnName("Address_Country");
      });

        // JSONB for WorkHours (e.g., {"Monday": "09:00-23:00"})
        builder.Property(c => c.WorkHours)
       .HasColumnType("nvarchar(max)")
                  .HasConversion(
        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
          v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>());
    }
}
