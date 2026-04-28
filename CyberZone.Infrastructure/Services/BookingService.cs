using CyberZone.Application.Common;
using CyberZone.Application.DTOs;
using CyberZone.Application.Interfaces;
using CyberZone.Domain.Entities;
using CyberZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberZone.Infrastructure.Services;

public class BookingService : IBookingService
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentService _payment;
    private readonly ICacheService _cache;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        IApplicationDbContext context,
        IPaymentService payment,
        ICacheService cache,
        ILogger<BookingService> logger)
    {
        _context = context;
        _payment = payment;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<BookNowDto>> PrepareFormAsync(Guid userId, Guid clubId, Guid hardwareId)
    {
        var hardware = await _context.Hardwares
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == hardwareId && h.ClubId == clubId);

        if (hardware is null)
            return Result.Failure<BookNowDto>("PC не знайдено для цього клубу.");

        if (hardware.Status != HardwareStatus.Available)
            return Result.Failure<BookNowDto>("PC зараз недоступний для бронювання.");

        var club = await _context.Clubs.AsNoTracking().FirstOrDefaultAsync(c => c.Id == clubId);
        if (club is null)
            return Result.Failure<BookNowDto>("Клуб не знайдено.");

        var tariffs = await _context.Tariffs
            .AsNoTracking()
            .Where(t => t.ClubId == clubId)
            .OrderBy(t => t.PricePerHour)
            .ToListAsync();

        if (tariffs.Count == 0)
            return Result.Failure<BookNowDto>("У клубі не налаштовано жодного тарифу.");

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return Result.Failure<BookNowDto>("Користувача не знайдено.");

        return Result.Success(new BookNowDto
        {
            ClubId = clubId,
            ClubName = club.Name,
            HardwareId = hardware.Id,
            PcNumber = hardware.PcNumber,
            TariffId = tariffs[0].Id,
            StartTime = DateTime.Now,
            Hours = 1,
            AvailableTariffs = tariffs.Select(t => new TariffOption
            {
                Id = t.Id,
                Name = t.Name,
                Type = t.Type,
                PricePerHour = t.PricePerHour
            }).ToList(),
            UserBalance = user.Balance
        });
    }

    public async Task<Result<Guid>> CreateAsync(Guid userId, BookNowDto dto)
    {
        if (dto.Hours is < 1 or > 24)
            return Result.Failure<Guid>("Тривалість має бути від 1 до 24 годин.");

        if (dto.StartTime < DateTime.UtcNow.AddMinutes(-5))
            return Result.Failure<Guid>("Час початку не може бути в минулому.");

        var hardware = await _context.Hardwares
            .FirstOrDefaultAsync(h => h.Id == dto.HardwareId && h.ClubId == dto.ClubId);
        if (hardware is null)
            return Result.Failure<Guid>("PC не знайдено для цього клубу.");

        if (hardware.Status != HardwareStatus.Available)
            return Result.Failure<Guid>("PC зараз недоступний для бронювання.");

        var tariff = await _context.Tariffs
            .FirstOrDefaultAsync(t => t.Id == dto.TariffId && t.ClubId == dto.ClubId);
        if (tariff is null)
            return Result.Failure<Guid>("Обраний тариф не належить цьому клубу.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return Result.Failure<Guid>("Користувача не знайдено.");

        var startUtc = dto.StartTime.Kind == DateTimeKind.Utc ? dto.StartTime : dto.StartTime.ToUniversalTime();
        var endUtc = startUtc.AddHours(dto.Hours);
        var cost = Math.Round(tariff.PricePerHour * dto.Hours, 2);

        if (user.Balance < cost)
            return Result.Failure<Guid>($"Недостатньо коштів. Потрібно {cost:0.00} ₴, доступно {user.Balance:0.00} ₴.");

        // Overlap: any active booking for the same PC that intersects [startUtc, endUtc).
        // Pending / Confirmed / Active rezervуют PC. Cancelled and Completed уже не блокують.
        // Active з минулою EndTime — це "висяк", теж ігноруємо (session вже могли завершити завчасно
        // без оновлення booking.Status).
        var now = DateTime.UtcNow;
        var hasOverlap = await _context.Bookings.AnyAsync(b =>
            b.HardwareId == dto.HardwareId
            && b.Status != BookingStatus.Cancelled
            && b.Status != BookingStatus.Completed
            && !(b.Status == BookingStatus.Active && b.EndTime <= now)
            && b.StartTime < endUtc
            && b.EndTime > startUtc);

        if (hasOverlap)
            return Result.Failure<Guid>("На цей час PC уже заброньовано. Оберіть інший інтервал.");

        var booking = new Booking
        {
            UserId = userId,
            HardwareId = dto.HardwareId,
            TariffId = dto.TariffId,
            StartTime = startUtc,
            EndTime = endUtc,
            Status = BookingStatus.Pending,
            Notes = dto.Notes
        };
        _context.Bookings.Add(booking);

        await _payment.ChargeSessionAsync(userId, booking.Id, cost);
        await _context.SaveChangesAsync();

        _cache.Remove(CacheKeys.ClubMap(dto.ClubId));
        _cache.Remove(CacheKeys.ClubDetails(dto.ClubId));

        _logger.LogInformation(
            "Booking {BookingId} created: user {UserId}, PC {HardwareId}, {Start}–{End}, cost {Cost}",
            booking.Id, userId, dto.HardwareId, startUtc, endUtc, cost);

        return Result.Success(booking.Id);
    }
}
