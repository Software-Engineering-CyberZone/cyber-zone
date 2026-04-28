using CyberZone.Application.DTOs;
using CyberZone.Application.Interfaces;
using CyberZone.Domain.Entities;
using CyberZone.Domain.Enums;
using CyberZone.Infrastructure.Services;
using CyberZone.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CyberZone.Tests.Infrastructure.Services;

public class BookingServiceTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IPaymentService> _mockPayment;
    private readonly BookingService _service;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _clubId = Guid.NewGuid();
    private readonly Guid _hardwareId = Guid.NewGuid();
    private readonly Guid _tariffId = Guid.NewGuid();

    public BookingServiceTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockPayment = new Mock<IPaymentService>();
        var logger = new Mock<ILogger<BookingService>>();
        _service = new BookingService(_mockContext.Object, _mockPayment.Object, new NoOpCacheService(), logger.Object);
    }

    // --- Fixtures ---

    private User CreateUser(decimal balance = 1000m) =>
        new() { Id = _userId, UserName = "testuser", Balance = balance };

    private Hardware CreateHardware(HardwareStatus status = HardwareStatus.Available) =>
        new() { Id = _hardwareId, ClubId = _clubId, PcNumber = "1", Status = status };

    private Tariff CreateTariff(decimal price = 100m) =>
        new() { Id = _tariffId, ClubId = _clubId, Name = "Standard", Type = TariffType.Hourly, PricePerHour = price };

    private Club CreateClub() =>
        new() { Id = _clubId, Name = "CyberPro Arena" };

    private void Setup(
        List<User>? users = null,
        List<Club>? clubs = null,
        List<Hardware>? hardwares = null,
        List<Tariff>? tariffs = null,
        List<Booking>? bookings = null)
    {
        _mockContext.Setup(c => c.Users).Returns(MockDbSetHelper.CreateMockDbSet(users ?? [CreateUser()]).Object);
        _mockContext.Setup(c => c.Clubs).Returns(MockDbSetHelper.CreateMockDbSet(clubs ?? [CreateClub()]).Object);
        _mockContext.Setup(c => c.Hardwares).Returns(MockDbSetHelper.CreateMockDbSet(hardwares ?? [CreateHardware()]).Object);
        _mockContext.Setup(c => c.Tariffs).Returns(MockDbSetHelper.CreateMockDbSet(tariffs ?? [CreateTariff()]).Object);
        _mockContext.Setup(c => c.Bookings).Returns(MockDbSetHelper.CreateMockDbSet(bookings ?? []).Object);
    }

    private BookNowDto ValidDto(int hours = 2) => new()
    {
        ClubId = _clubId,
        HardwareId = _hardwareId,
        TariffId = _tariffId,
        StartTime = DateTime.UtcNow.AddHours(1),
        Hours = hours
    };

    // ---------- PrepareFormAsync ----------

    [Fact]
    public async Task PrepareFormAsync_ValidInput_ReturnsSuccessWithTariffs()
    {
        Setup(tariffs: [CreateTariff(100m), new Tariff { Id = Guid.NewGuid(), ClubId = _clubId, Name = "Night", Type = TariffType.Night, PricePerHour = 75m }]);

        var result = await _service.PrepareFormAsync(_userId, _clubId, _hardwareId);

        result.IsSuccess.Should().BeTrue();
        result.Value.AvailableTariffs.Should().HaveCount(2);
        result.Value.PcNumber.Should().Be("1");
        result.Value.ClubName.Should().Be("CyberPro Arena");
    }

    [Fact]
    public async Task PrepareFormAsync_ValidInput_ReturnsUserBalance()
    {
        Setup(users: [CreateUser(balance: 450m)]);

        var result = await _service.PrepareFormAsync(_userId, _clubId, _hardwareId);

        result.Value.UserBalance.Should().Be(450m);
    }

    [Fact]
    public async Task PrepareFormAsync_HardwareNotFound_ReturnsFailure()
    {
        Setup(hardwares: []);

        var result = await _service.PrepareFormAsync(_userId, _clubId, _hardwareId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("PC");
    }

    [Fact]
    public async Task PrepareFormAsync_HardwareBusy_ReturnsFailure()
    {
        Setup(hardwares: [CreateHardware(HardwareStatus.Busy)]);

        var result = await _service.PrepareFormAsync(_userId, _clubId, _hardwareId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("недоступний");
    }

    [Fact]
    public async Task PrepareFormAsync_NoTariffs_ReturnsFailure()
    {
        Setup(tariffs: []);

        var result = await _service.PrepareFormAsync(_userId, _clubId, _hardwareId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("тариф");
    }

    [Fact]
    public async Task PrepareFormAsync_UserNotFound_ReturnsFailure()
    {
        Setup(users: []);

        var result = await _service.PrepareFormAsync(_userId, _clubId, _hardwareId);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task PrepareFormAsync_ClubMismatch_ReturnsFailure()
    {
        var otherClubHw = new Hardware { Id = _hardwareId, ClubId = Guid.NewGuid(), PcNumber = "1", Status = HardwareStatus.Available };
        Setup(hardwares: [otherClubHw]);

        var result = await _service.PrepareFormAsync(_userId, _clubId, _hardwareId);

        result.IsFailure.Should().BeTrue();
    }

    // ---------- CreateAsync: validation ----------

    [Fact]
    public async Task CreateAsync_ZeroHours_ReturnsFailure()
    {
        Setup();
        var dto = ValidDto(hours: 0);

        var result = await _service.CreateAsync(_userId, dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Тривалість");
    }

    [Fact]
    public async Task CreateAsync_MoreThan24Hours_ReturnsFailure()
    {
        Setup();
        var dto = ValidDto(hours: 25);

        var result = await _service.CreateAsync(_userId, dto);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_StartTimeInPast_ReturnsFailure()
    {
        Setup();
        var dto = ValidDto();
        dto.StartTime = DateTime.UtcNow.AddHours(-2);

        var result = await _service.CreateAsync(_userId, dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("минулому");
    }

    [Fact]
    public async Task CreateAsync_HardwareNotFound_ReturnsFailure()
    {
        Setup(hardwares: []);

        var result = await _service.CreateAsync(_userId, ValidDto());

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_HardwareBusy_ReturnsFailure()
    {
        Setup(hardwares: [CreateHardware(HardwareStatus.Busy)]);

        var result = await _service.CreateAsync(_userId, ValidDto());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("недоступний");
    }

    [Fact]
    public async Task CreateAsync_TariffFromDifferentClub_ReturnsFailure()
    {
        var foreignTariff = new Tariff { Id = _tariffId, ClubId = Guid.NewGuid(), Name = "Cheap", PricePerHour = 10m };
        Setup(tariffs: [foreignTariff]);

        var result = await _service.CreateAsync(_userId, ValidDto());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("тариф");
    }

    [Fact]
    public async Task CreateAsync_InsufficientBalance_ReturnsFailure()
    {
        Setup(users: [CreateUser(balance: 50m)], tariffs: [CreateTariff(price: 100m)]);
        var dto = ValidDto(hours: 2); // cost = 200, balance = 50

        var result = await _service.CreateAsync(_userId, dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Недостатньо");
    }

    [Fact]
    public async Task CreateAsync_InsufficientBalance_DoesNotCharge()
    {
        Setup(users: [CreateUser(balance: 50m)], tariffs: [CreateTariff(price: 100m)]);

        await _service.CreateAsync(_userId, ValidDto(hours: 2));

        _mockPayment.Verify(p => p.ChargeSessionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- CreateAsync: overlap ----------

    [Fact]
    public async Task CreateAsync_OverlappingPendingBooking_ReturnsFailure()
    {
        var dto = ValidDto(hours: 2); // [now+1h .. now+3h]
        var overlap = new Booking
        {
            HardwareId = _hardwareId,
            StartTime = dto.StartTime.AddMinutes(30),
            EndTime = dto.StartTime.AddHours(1).AddMinutes(30),
            Status = BookingStatus.Pending
        };
        Setup(bookings: [overlap]);

        var result = await _service.CreateAsync(_userId, dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("заброньовано");
    }

    [Fact]
    public async Task CreateAsync_CancelledBooking_DoesNotBlock()
    {
        var dto = ValidDto(hours: 2);
        var cancelled = new Booking
        {
            HardwareId = _hardwareId,
            StartTime = dto.StartTime,
            EndTime = dto.StartTime.AddHours(2),
            Status = BookingStatus.Cancelled
        };
        Setup(bookings: [cancelled]);

        var result = await _service.CreateAsync(_userId, dto);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_CompletedBooking_DoesNotBlock()
    {
        var dto = ValidDto(hours: 2);
        var completed = new Booking
        {
            HardwareId = _hardwareId,
            StartTime = dto.StartTime,
            EndTime = dto.StartTime.AddHours(2),
            Status = BookingStatus.Completed
        };
        Setup(bookings: [completed]);

        var result = await _service.CreateAsync(_userId, dto);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_BookingOnDifferentPc_DoesNotBlock()
    {
        var dto = ValidDto(hours: 2);
        var otherPc = new Booking
        {
            HardwareId = Guid.NewGuid(),
            StartTime = dto.StartTime,
            EndTime = dto.StartTime.AddHours(2),
            Status = BookingStatus.Pending
        };
        Setup(bookings: [otherPc]);

        var result = await _service.CreateAsync(_userId, dto);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ActiveBookingWithPastEndTime_DoesNotBlock()
    {
        // User's previous session was ended early but booking.Status stayed Active.
        // New booking for the same PC in the future must still succeed.
        var dto = ValidDto(hours: 2);
        var stale = new Booking
        {
            HardwareId = _hardwareId,
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow.AddMinutes(-5),
            Status = BookingStatus.Active
        };
        Setup(bookings: [stale]);

        var result = await _service.CreateAsync(_userId, dto);

        result.IsSuccess.Should().BeTrue();
    }

    // ---------- CreateAsync: happy path ----------

    [Fact]
    public async Task CreateAsync_ValidInput_ReturnsSuccessWithBookingId()
    {
        Setup();

        var result = await _service.CreateAsync(_userId, ValidDto());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateAsync_ValidInput_ChargesUserForCorrectCost()
    {
        Setup(tariffs: [CreateTariff(price: 75m)]);

        await _service.CreateAsync(_userId, ValidDto(hours: 3));

        _mockPayment.Verify(
            p => p.ChargeSessionAsync(_userId, It.IsAny<Guid>(), 225m, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ValidInput_SavesChanges()
    {
        Setup();

        await _service.CreateAsync(_userId, ValidDto());

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
