using CyberZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberZone.Infrastructure.Persistence.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
  .HasMaxLength(200);

        builder.Property(m => m.Description)
  .HasMaxLength(1000);

        builder.Property(m => m.Price)
        .HasPrecision(18, 2);

        builder.Property(m => m.Category)
     .HasMaxLength(100);

        builder.HasOne(m => m.Club)
            .WithMany(c => c.MenuItems)
               .HasForeignKey(m => m.ClubId)
                  .OnDelete(DeleteBehavior.Restrict);
    }
}
