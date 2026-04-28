using CyberZone.Domain.Entities;

namespace CyberZone.Application.Interfaces;

public interface IPaymentService
{
    Task<Transaction> TopUpAsync(Guid userId, decimal amount, string? description = null, CancellationToken ct = default);
    Task<Transaction> ChargeSessionAsync(Guid userId, Guid sessionId, decimal amount, CancellationToken ct = default);
    Task<Transaction> PayOrderAsync(Guid userId, Guid orderId, decimal amount, CancellationToken ct = default);
    Task<Transaction> RefundAsync(Guid userId, decimal amount, Guid? referenceId = null, string? description = null, CancellationToken ct = default);
}
