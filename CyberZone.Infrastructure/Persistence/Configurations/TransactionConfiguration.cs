using CyberZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberZone.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);

        // Transaction is immutable — configure all properties as read-only after insert
        builder.Property(t => t.Type)
         .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.Amount)
         .HasPrecision(18, 2);

        builder.Property(t => t.Description)
             .HasMaxLength(500);

        builder.HasOne(t => t.User)
 .WithMany(u => u.Transactions)
    .HasForeignKey(t => t.UserId)
    .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.ReferenceId);
        builder.HasIndex(t => t.TransactionDate);
    }
}
