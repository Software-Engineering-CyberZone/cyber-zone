using CyberZone.Domain.Entities;
using CyberZone.Domain.Enums;
using CyberZone.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CyberZone.Infrastructure.Services;

/// <summary>
/// Handles wallet operations: top-ups, session charges, order payments, and refunds.
/// </summary>
public class PaymentService
{
    private readonly CyberZoneDbContext _context;

    public PaymentService(CyberZoneDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Adds funds to a user's balance and records a TopUp transaction.
    /// </summary>
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

        return transaction;
    }

    /// <summary>
    /// Charges the user's balance for a completed gaming session.
    /// </summary>
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

        return transaction;
    }

    /// <summary>
    /// Charges the user's balance for an order and records an OrderPayment transaction.
    /// </summary>
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

        return transaction;
    }

    /// <summary>
    /// Refunds a specified amount to the user's balance.
    /// </summary>
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

        return transaction;
    }
}
