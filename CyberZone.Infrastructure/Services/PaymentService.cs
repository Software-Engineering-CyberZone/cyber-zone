using CyberZone.Application.Interfaces;
using CyberZone.Domain.Entities;
using CyberZone.Domain.Enums;
using CyberZone.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberZone.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly CyberZoneDbContext _context;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(CyberZoneDbContext context, ILogger<PaymentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Transaction> TopUpAsync(Guid userId, decimal amount, string? description = null, CancellationToken ct = default)
    {
        var user = await _context.Users.FindAsync([userId], ct)
            ?? throw new InvalidOperationException($"User {userId} not found.");

        user.Balance += amount;

        var transaction = new Transaction
        {
            Type = TransactionType.TopUp,
            Amount = amount,
            Description = description ?? "Wallet top-up",
            UserId = userId,
            TransactionDate = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Top-up {Amount} for user {UserId}. New balance: {Balance}", amount, userId, user.Balance);
        return transaction;
    }

    public async Task<Transaction> ChargeSessionAsync(Guid userId, Guid sessionId, decimal amount, CancellationToken ct = default)
    {
        var user = await _context.Users.FindAsync([userId], ct)
            ?? throw new InvalidOperationException($"User {userId} not found.");

        if (user.Balance < amount)
            throw new InvalidOperationException("Insufficient balance for session charge.");

        user.Balance -= amount;

        var transaction = new Transaction
        {
            Type = TransactionType.SessionCharge,
            Amount = amount,
            Description = "Gaming session charge",
            UserId = userId,
            ReferenceId = sessionId,
            TransactionDate = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Session charge {Amount} for user {UserId}, session {SessionId}", amount, userId, sessionId);
        return transaction;
    }

    public async Task<Transaction> PayOrderAsync(Guid userId, Guid orderId, decimal amount, CancellationToken ct = default)
    {
        var user = await _context.Users.FindAsync([userId], ct)
            ?? throw new InvalidOperationException($"User {userId} not found.");

        if (user.Balance < amount)
            throw new InvalidOperationException("Insufficient balance for order payment.");

        user.Balance -= amount;

        var transaction = new Transaction
        {
            Type = TransactionType.OrderPayment,
            Amount = amount,
            Description = "Order payment",
            UserId = userId,
            ReferenceId = orderId,
            TransactionDate = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Order payment {Amount} for user {UserId}, order {OrderId}", amount, userId, orderId);
        return transaction;
    }

    public async Task<Transaction> RefundAsync(Guid userId, decimal amount, Guid? referenceId = null, string? description = null, CancellationToken ct = default)
    {
        var user = await _context.Users.FindAsync([userId], ct)
            ?? throw new InvalidOperationException($"User {userId} not found.");

        user.Balance += amount;

        var transaction = new Transaction
        {
            Type = TransactionType.Refund,
            Amount = amount,
            Description = description ?? "Refund",
            UserId = userId,
            ReferenceId = referenceId,
            TransactionDate = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Refund {Amount} for user {UserId}. Reference: {ReferenceId}", amount, userId, referenceId);
        return transaction;
    }
}
