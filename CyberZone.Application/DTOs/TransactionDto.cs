using CyberZone.Domain.Enums;

namespace CyberZone.Application.DTOs;

public class TransactionDto
{
    public Guid Id { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public Guid UserId { get; set; }
    public Guid? ReferenceId { get; set; }
}
