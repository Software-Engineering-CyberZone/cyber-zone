using CyberZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberZone.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);

        // Unique constraint: one review per user per club
        builder.HasIndex(r => new { r.UserId, r.ClubId })
            .IsUnique();

        // Rating must be between 1 and 5
        builder.Property(r => r.Rating)
                    .IsRequired();

        builder.ToTable(t => t.HasCheckConstraint("CK_Review_Rating", "[Rating] >= 1 AND [Rating] <= 5"));

        builder.Property(r => r.Comment)
     .HasMaxLength(2000);

        builder.HasOne(r => r.User)
       .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Club)
         .WithMany(c => c.Reviews)
            .HasForeignKey(r => r.ClubId)
       .OnDelete(DeleteBehavior.Restrict);
    }
}
