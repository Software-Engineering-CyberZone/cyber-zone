using CyberZone.Domain.Entities;
using CyberZone.Domain.Enums;
using CyberZone.Infrastructure.Persistence;
using CyberZone.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CyberZone.Tests.Infrastructure.Services;

public class PaymentServiceTests : IDisposable
{
    private readonly CyberZoneDbContext _context;
    private readonly PaymentService _paymentService;

    public PaymentServiceTests()
    {
        var options = new DbContextOptionsBuilder<CyberZoneDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CyberZoneDbContext(options);
        _paymentService = new PaymentService(_context);
    }

    private async Task<User> SeedUserAsync(decimal balance = 0m)
    {
        var user = new User
        {
            UserName = "testuser",
            Email = "test@test.com",
            FullName = "Test User",
            Balance = balance
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    // --- TopUpAsync ---

    [Fact]
    public async Task TopUpAsync_ValidUser_IncreasesBalance()
    {
        var user = await SeedUserAsync(balance: 100m);

        await _paymentService.TopUpAsync(user.Id, 50m);

        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.Balance.Should().Be(150m);
    }

    [Fact]
    public async Task TopUpAsync_ValidUser_CreatesTopUpTransaction()
    {
        var user = await SeedUserAsync();

        var transaction = await _paymentService.TopUpAsync(user.Id, 200m, "Test top-up");

        transaction.Type.Should().Be(TransactionType.TopUp);
        transaction.Amount.Should().Be(200m);
        transaction.Description.Should().Be("Test top-up");
        transaction.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task TopUpAsync_DefaultDescription_IsWalletTopUp()
    {
        var user = await SeedUserAsync();

        var transaction = await _paymentService.TopUpAsync(user.Id, 100m);

        transaction.Description.Should().Be("Wallet top-up");
    }

    [Fact]
    public async Task TopUpAsync_NonExistentUser_ThrowsInvalidOperationException()
    {
        var act = () => _paymentService.TopUpAsync(Guid.NewGuid(), 100m);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task TopUpAsync_TransactionIsPersisted()
    {
        var user = await SeedUserAsync();

        await _paymentService.TopUpAsync(user.Id, 50m);

        var transactions = await _context.Transactions.ToListAsync();
        transactions.Should().HaveCount(1);
    }

    // --- ChargeSessionAsync ---

    [Fact]
    public async Task ChargeSessionAsync_SufficientBalance_DeductsAmount()
    {
        var user = await SeedUserAsync(balance: 500m);
        var sessionId = Guid.NewGuid();

        await _paymentService.ChargeSessionAsync(user.Id, sessionId, 200m);

        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.Balance.Should().Be(300m);
    }

    [Fact]
    public async Task ChargeSessionAsync_SufficientBalance_CreatesSessionChargeTransaction()
    {
        var user = await SeedUserAsync(balance: 500m);
        var sessionId = Guid.NewGuid();

        var transaction = await _paymentService.ChargeSessionAsync(user.Id, sessionId, 200m);

        transaction.Type.Should().Be(TransactionType.SessionCharge);
        transaction.Amount.Should().Be(200m);
        transaction.ReferenceId.Should().Be(sessionId);
        transaction.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task ChargeSessionAsync_InsufficientBalance_ThrowsInvalidOperationException()
    {
        var user = await SeedUserAsync(balance: 50m);

        var act = () => _paymentService.ChargeSessionAsync(user.Id, Guid.NewGuid(), 100m);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Insufficient balance*");
    }

    [Fact]
    public async Task ChargeSessionAsync_ExactBalance_Succeeds()
    {
        var user = await SeedUserAsync(balance: 100m);

        await _paymentService.ChargeSessionAsync(user.Id, Guid.NewGuid(), 100m);

        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.Balance.Should().Be(0m);
    }

    [Fact]
    public async Task ChargeSessionAsync_NonExistentUser_ThrowsInvalidOperationException()
    {
        var act = () => _paymentService.ChargeSessionAsync(Guid.NewGuid(), Guid.NewGuid(), 100m);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    // --- PayOrderAsync ---

    [Fact]
    public async Task PayOrderAsync_SufficientBalance_DeductsAmount()
    {
        var user = await SeedUserAsync(balance: 300m);
        var orderId = Guid.NewGuid();

        await _paymentService.PayOrderAsync(user.Id, orderId, 100m);

        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.Balance.Should().Be(200m);
    }

    [Fact]
    public async Task PayOrderAsync_SufficientBalance_CreatesOrderPaymentTransaction()
    {
        var user = await SeedUserAsync(balance: 300m);
        var orderId = Guid.NewGuid();

        var transaction = await _paymentService.PayOrderAsync(user.Id, orderId, 100m);

        transaction.Type.Should().Be(TransactionType.OrderPayment);
        transaction.Amount.Should().Be(100m);
        transaction.ReferenceId.Should().Be(orderId);
    }

    [Fact]
    public async Task PayOrderAsync_InsufficientBalance_ThrowsInvalidOperationException()
    {
        var user = await SeedUserAsync(balance: 10m);

        var act = () => _paymentService.PayOrderAsync(user.Id, Guid.NewGuid(), 50m);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Insufficient balance*");
    }

    [Fact]
    public async Task PayOrderAsync_InsufficientBalance_DoesNotCreateTransaction()
    {
        var user = await SeedUserAsync(balance: 10m);

        try { await _paymentService.PayOrderAsync(user.Id, Guid.NewGuid(), 50m); }
        catch { /* expected */ }

        var transactions = await _context.Transactions.ToListAsync();
        transactions.Should().BeEmpty();
    }

    // --- RefundAsync ---

    [Fact]
    public async Task RefundAsync_ValidUser_IncreasesBalance()
    {
        var user = await SeedUserAsync(balance: 100m);

        await _paymentService.RefundAsync(user.Id, 50m);

        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.Balance.Should().Be(150m);
    }

    [Fact]
    public async Task RefundAsync_ValidUser_CreatesRefundTransaction()
    {
        var user = await SeedUserAsync();
        var refId = Guid.NewGuid();

        var transaction = await _paymentService.RefundAsync(user.Id, 75m, refId, "Session refund");

        transaction.Type.Should().Be(TransactionType.Refund);
        transaction.Amount.Should().Be(75m);
        transaction.ReferenceId.Should().Be(refId);
        transaction.Description.Should().Be("Session refund");
    }

    [Fact]
    public async Task RefundAsync_DefaultDescription_IsRefund()
    {
        var user = await SeedUserAsync();

        var transaction = await _paymentService.RefundAsync(user.Id, 25m);

        transaction.Description.Should().Be("Refund");
    }

    [Fact]
    public async Task RefundAsync_NullReferenceId_IsAllowed()
    {
        var user = await SeedUserAsync();

        var transaction = await _paymentService.RefundAsync(user.Id, 25m);

        transaction.ReferenceId.Should().BeNull();
    }

    [Fact]
    public async Task RefundAsync_NonExistentUser_ThrowsInvalidOperationException()
    {
        var act = () => _paymentService.RefundAsync(Guid.NewGuid(), 100m);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
