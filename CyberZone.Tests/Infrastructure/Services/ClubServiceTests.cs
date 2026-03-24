using CyberZone.Application.Interfaces;
using CyberZone.Domain.Entities;
using CyberZone.Domain.Enums;
using CyberZone.Domain.ValueObjects;
using CyberZone.Infrastructure.Persistence;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace CyberZone.Tests.Infrastructure.Services;

public class ClubServiceTests : IDisposable
{
    private readonly CyberZoneDbContext _context;
    private readonly ClubService _clubService;

    public ClubServiceTests()
    {
        var options = new DbContextOptionsBuilder<CyberZoneDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CyberZoneDbContext(options);
        var logger = new Mock<ILogger<ClubService>>();
        _clubService = new ClubService(_context, logger.Object);
    }

    private async Task<Club> SeedClubAsync()
    {
        var club = new Club
        {
            Name = "CyberPro Arena",
            Address = new Address
            {
                City = "Kyiv",
                Street = "Khreshchatyk St.",
                State = "Kyiv region",
                ZipCode = "01000",
                Country = "Ukraine"
            },
            Phone = "+380991234567",
            Email = "info@cyberpro.ua",
            Rating = 4.8,
            WorkHours = new Dictionary<string, string>
            {
                ["Monday"] = "09:00-23:00",
                ["Tuesday"] = "09:00-23:00"
            }
        };

        club.Hardwares.Add(new Hardware
        {
            PcNumber = "PC-01",
            Status = HardwareStatus.Available,
            Specs = new Dictionary<string, string>
            {
                ["CPU"] = "Intel i7-13700K",
                ["GPU"] = "RTX 4070",
                ["RAM"] = "32GB"
            }
        });

        club.Hardwares.Add(new Hardware
        {
            PcNumber = "PC-02",
            Status = HardwareStatus.Busy,
            Specs = new Dictionary<string, string>
            {
                ["CPU"] = "AMD Ryzen 7 7800X3D",
                ["GPU"] = "RTX 4060",
                ["RAM"] = "16GB"
            }
        });

        club.Tariffs.Add(new Tariff
        {
            Name = "Standard",
            Type = TariffType.Hourly,
            PricePerHour = 100m,
            Description = "Standard hourly rate"
        });

        club.Tariffs.Add(new Tariff
        {
            Name = "Night Owl",
            Type = TariffType.Night,
            PricePerHour = 75m,
            Description = "Night discount rate"
        });

        club.MenuItems.Add(new MenuItem
        {
            Name = "Coca-Cola",
            Description = "330ml can",
            Price = 35m,
            Category = "Drinks",
            IsAvailable = true
        });

        club.MenuItems.Add(new MenuItem
        {
            Name = "Pizza Margherita",
            Description = "Classic pizza",
            Price = 150m,
            Category = "Food",
            IsAvailable = true
        });

        _context.Clubs.Add(club);
        await _context.SaveChangesAsync();
        return club;
    }

    // --- GetClubDetailsAsync Success ---

    [Fact]
    public async Task GetClubDetailsAsync_ExistingClub_ReturnsSuccess()
    {
        // Arrange
        var club = await SeedClubAsync();

        // Act
        var result = await _clubService.GetClubDetailsAsync(club.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetClubDetailsAsync_ExistingClub_ReturnsCorrectBasicInfo()
    {
        // Arrange
        var club = await SeedClubAsync();

        // Act
        var result = await _clubService.GetClubDetailsAsync(club.Id);

        // Assert
        var dto = result.Value;
        dto.Id.Should().Be(club.Id);
        dto.Name.Should().Be("CyberPro Arena");
        dto.Phone.Should().Be("+380991234567");
        dto.Email.Should().Be("info@cyberpro.ua");
        dto.Rating.Should().Be(4.8);
    }

    [Fact]
    public async Task GetClubDetailsAsync_ExistingClub_ReturnsHardwares()
    {
        // Arrange
        var club = await SeedClubAsync();

        // Act
        var result = await _clubService.GetClubDetailsAsync(club.Id);

        // Assert
        result.Value.Hardwares.Should().HaveCount(2);
        result.Value.Hardwares.Should().Contain(h => h.PcNumber == "PC-01" && h.Status == HardwareStatus.Available);
        result.Value.Hardwares.Should().Contain(h => h.PcNumber == "PC-02" && h.Status == HardwareStatus.Busy);
    }

    [Fact]
    public async Task GetClubDetailsAsync_ExistingClub_ReturnsTariffs()
    {
        // Arrange
        var club = await SeedClubAsync();

        // Act
        var result = await _clubService.GetClubDetailsAsync(club.Id);

        // Assert
        result.Value.Tariffs.Should().HaveCount(2);
        result.Value.Tariffs.Should().Contain(t => t.Name == "Standard" && t.PricePerHour == 100m);
        result.Value.Tariffs.Should().Contain(t => t.Name == "Night Owl" && t.PricePerHour == 75m);
    }

    [Fact]
    public async Task GetClubDetailsAsync_ExistingClub_ReturnsMenuItems()
    {
        // Arrange
        var club = await SeedClubAsync();

        // Act
        var result = await _clubService.GetClubDetailsAsync(club.Id);

        // Assert
        result.Value.MenuItems.Should().HaveCount(2);
        result.Value.MenuItems.Should().Contain(m => m.Name == "Coca-Cola" && m.Price == 35m);
        result.Value.MenuItems.Should().Contain(m => m.Name == "Pizza Margherita" && m.Price == 150m);
    }

    [Fact]
    public async Task GetClubDetailsAsync_ExistingClub_ReturnsHardwareSpecs()
    {
        // Arrange
        var club = await SeedClubAsync();

        // Act
        var result = await _clubService.GetClubDetailsAsync(club.Id);

        // Assert
        var pc01 = result.Value.Hardwares.First(h => h.PcNumber == "PC-01");
        pc01.Specs.Should().ContainKey("CPU");
        pc01.Specs["GPU"].Should().Be("RTX 4070");
    }

    [Fact]
    public async Task GetClubDetailsAsync_ExistingClub_ReturnsWorkHours()
    {
        // Arrange
        var club = await SeedClubAsync();

        // Act
        var result = await _clubService.GetClubDetailsAsync(club.Id);

        // Assert
        result.Value.WorkHours.Should().HaveCount(2);
        result.Value.WorkHours["Monday"].Should().Be("09:00-23:00");
    }

    // --- GetClubDetailsAsync Failure ---

    [Fact]
    public async Task GetClubDetailsAsync_NonExistentId_ReturnsFailure()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _clubService.GetClubDetailsAsync(nonExistentId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task GetClubDetailsAsync_NonExistentId_ValueAccessThrows()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _clubService.GetClubDetailsAsync(nonExistentId);

        // Assert
        var act = () => result.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    // --- GetClubsForCatalogAsync ---

    [Fact]
    public async Task GetClubsForCatalogAsync_ReturnsSuccessWithClubs()
    {
        // Arrange
        await SeedClubAsync();

        // Act
        var result = await _clubService.GetClubsForCatalogAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetClubsForCatalogAsync_EmptyDb_ReturnsSuccessWithEmpty()
    {
        // Act
        var result = await _clubService.GetClubsForCatalogAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetClubsForCatalogAsync_ReturnsMinPrice()
    {
        // Arrange
        await SeedClubAsync();

        // Act
        var result = await _clubService.GetClubsForCatalogAsync();

        // Assert
        result.Value.First().MinPrice.Should().Be(75m);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
