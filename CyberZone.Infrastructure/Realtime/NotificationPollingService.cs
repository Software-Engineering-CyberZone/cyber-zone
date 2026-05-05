using CyberZone.Application.DTOs;
using CyberZone.Domain.Enums;
using CyberZone.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CyberZone.Infrastructure.Realtime;

public class NotificationPollingService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan SessionWarningWindowStart = TimeSpan.FromMinutes(4);
    private static readonly TimeSpan SessionWarningWindowEnd = TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationPollingService> _logger;

    private readonly HashSet<Guid> _sentSessionWarningIds = [];
    private readonly HashSet<Guid> _sentBookingIds = [];
    private readonly HashSet<Guid> _sentOrderIds = [];
    private DateTime _lastPollUtc = DateTime.UtcNow;

    public NotificationPollingService(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationPollingService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationPollingService started, polling every {Seconds}s", PollInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notification poll iteration failed");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task PollOnceAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CyberZoneDbContext>();
        var hub = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();

        var now = DateTime.UtcNow;
        var windowStart = now.Add(SessionWarningWindowStart);
        var windowEnd = now.Add(SessionWarningWindowEnd);

        var endingSessions = await db.GamingSessions
            .Include(s => s.Hardware)
            .Where(s => s.Status == SessionStatus.Active
                     && s.EndTime.HasValue
                     && s.EndTime.Value >= windowStart
                     && s.EndTime.Value <= windowEnd)
            .ToListAsync(ct);

        foreach (var session in endingSessions)
        {
            if (!_sentSessionWarningIds.Add(session.Id)) continue;

            var pc = session.Hardware?.PcNumber ?? "ПК";
            await hub.Clients.Group($"user-{session.UserId}").SendAsync(
                "notify",
                new NotificationDto
                {
                    Title = "Сесія закінчується",
                    Message = $"Ваша сесія на {pc} закінчується через 5 хв.",
                    Level = "warning"
                },
                ct);
        }

        var newBookings = await db.Bookings
            .Include(b => b.Hardware)
            .Include(b => b.User)
            .Where(b => b.CreatedAt > _lastPollUtc && b.Status == BookingStatus.Pending)
            .ToListAsync(ct);

        foreach (var booking in newBookings)
        {
            if (!_sentBookingIds.Add(booking.Id)) continue;
            if (booking.Hardware is null) continue;

            var clubId = booking.Hardware.ClubId;
            var userName = booking.User?.UserName ?? "клієнт";
            await hub.Clients.Group($"club-{clubId}").SendAsync(
                "notify",
                new NotificationDto
                {
                    Title = "Нове бронювання",
                    Message = $"{userName} забронював {booking.Hardware.PcNumber} на {booking.StartTime.ToLocalTime():HH:mm}.",
                    Level = "info"
                },
                ct);
        }

        var newOrders = await db.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(i => i.MenuItem)
            .Where(o => o.CreatedAt > _lastPollUtc && o.Status == OrderStatus.Pending)
            .ToListAsync(ct);

        foreach (var order in newOrders)
        {
            if (!_sentOrderIds.Add(order.Id)) continue;

            var firstItem = order.Items.FirstOrDefault();
            if (firstItem?.MenuItem is null) continue;

            var clubId = firstItem.MenuItem.ClubId;
            var itemSummary = string.Join(", ", order.Items
                .Where(i => i.MenuItem is not null)
                .Select(i => $"{i.MenuItem.Name} ×{i.Quantity}"));

            await hub.Clients.Group($"club-{clubId}").SendAsync(
                "notify",
                new NotificationDto
                {
                    Title = "Нове замовлення",
                    Message = $"Замовлення на {order.TotalAmount:0.##} грн: {itemSummary}",
                    Level = "success"
                },
                ct);
        }

        _lastPollUtc = now;

        if (_sentSessionWarningIds.Count > 10000) _sentSessionWarningIds.Clear();
        if (_sentBookingIds.Count > 10000) _sentBookingIds.Clear();
        if (_sentOrderIds.Count > 10000) _sentOrderIds.Clear();
    }
}
