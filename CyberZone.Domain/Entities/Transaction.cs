using CyberZone.Domain.Common;
using CyberZone.Domain.Enums;

namespace CyberZone.Domain.Entities;

/// <summary>
/// Immutable transaction record. Once created, it should not be modified.
/// </summary>
public class Transaction : EntityBase
{
    public TransactionType Type { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
    public DateTime TransactionDate { get; init; } = DateTime.UtcNow;

    public Guid UserId { get; init; }
    public User User { get; init; } = null!;

    /// <summary>
    /// Optional reference to the related order, session, etc.
    /// </summary>
    public Guid? ReferenceId { get; init; }
}
