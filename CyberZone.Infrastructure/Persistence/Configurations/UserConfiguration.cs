using CyberZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberZone.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(u => u.UserName)
   .IsUnique();

        builder.Property(u => u.Email)
            .IsRequired()
 .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
              .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.FullName)
      .HasMaxLength(200);

        builder.Property(u => u.Phone)
            .HasMaxLength(20);

        builder.Property(u => u.Role)
          .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(u => u.Balance)
       .HasPrecision(18, 2)
            .HasDefaultValue(0m);

        // JSONB for LinkedAccounts (Steam, Epic, Battle.net, etc.)
        builder.Property(u => u.LinkedAccounts)
            .HasColumnType("nvarchar(max)")
     .HasConversion(
  v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
      v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>());
    }
}
