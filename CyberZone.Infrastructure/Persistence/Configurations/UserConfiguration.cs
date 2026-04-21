using CyberZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberZone.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.FullName)
            .HasMaxLength(200);

        builder.Property(u => u.Balance)
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);

        builder.Property(u => u.Bio)
            .HasMaxLength(500);

        builder.Property(u => u.Phone)
            .HasMaxLength(20);

        builder.Property(u => u.Location)
            .HasMaxLength(200);

        builder.Property(u => u.WebsiteUrl)
            .HasMaxLength(500);

        builder.Property(u => u.ProfileImagePath)
            .HasMaxLength(500);

        // JSONB for LinkedAccounts (Steam, Epic, Battle.net, etc.)
        builder.Property(u => u.LinkedAccounts)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>());

        builder.HasOne(u => u.ManagedClub)
            .WithMany(c => c.StaffMembers)
            .HasForeignKey(u => u.ManagedClubId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
