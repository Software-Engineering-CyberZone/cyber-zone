using CyberZone.Application.Interfaces;
using CyberZone.Domain.Entities;
using CyberZone.Domain.Enums;
using CyberZone.Domain.ValueObjects;
using CyberZone.Tests.Helpers;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace CyberZone.Tests.Infrastructure.Services;

public class ClubServiceTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly ClubService _clubService;

    public ClubServiceTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        var logger = new Mock<ILogger<ClubService>>();
        _clubService = new ClubService(_mockContext.Object, logger.Object);
    }

    private static Club CreateTestClub()
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

        return club;
    }

    private void SetupClubs(List<Club> clubs)
    {
        var mockSet = MockDbSetHelper.CreateMockDbSet(clubs);
        _mockContext.Setup(c => c.Clubs).Returns(mockSet.Object);
    }

    // --- GetClubDetailsAsync Success ---

    [Fact]
    public async Task GetClubDetailsAsync_ExistingClub_ReturnsSuccess()
    {
        var club = CreateTestClub();
        SetupClubs([club]);

        var result = await _clubService.GetClubDetailsAsync(club.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetClubDetailsAsync_ExistingClub_ReturnsCorrectBasicInfo()
    {
        var club = CreateTestClub();
        SetupClubs([club]);

        var result = await _clubService.GetClubDetailsAsync(club.Id);

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
        var club = CreateTestClub();
        SetupClubs([club]);

        var result = await _clubService.GetClubDetailsAsync(club.Id);

        result.Value.Hardwares.Should().HaveCount(2);
        result.Value.Hardwares.Should().Contain(h => h.PcNumber == "PC-01" && h.Status == HardwareStatus.Available);
        result.Value.Hardwares.Should().Contain(h => h.PcNumber == "PC-02" && h.Status == HardwareStatus.Busy);
    }

    [Fact]
    public async Task GetClubDetailsAsync_ExistingClub_ReturnsTariffs()
    {
        var club = CreateTestClub();
        SetupClubs([club]);

        var result = await _clubService.GetClubDetailsAsync(club.Id);

        result.Value.Tariffs.Should().HaveCount(2);
        result.Value.Tariffs.Should().Contain(t => t.Name == "Standard" && t.PricePerHour == 100m);
        result.Value.Tariffs.Should().Contain(t => t.Name == "Night Owl" && t.PricePerHour == 75m);
    }

    [Fact]
    public async Task GetClubDetailsAsync_ExistingClub_ReturnsMenuItems()
    {
        var club = CreateTestClub();
        SetupClubs([club]);

        var result = await _clubService.GetClubDetailsAsync(club.Id);

        result.Value.MenuItems.Should().HaveCount(2);
        result.Value.MenuItems.Should().Contain(m => m.Name == "Coca-Cola" && m.Price == 35m);
        result.Value.MenuItems.Should().Contain(m => m.Name == "Pizza Margherita" && m.Price == 150m);
    }

    [Fact]
    public async Task GetClubDetailsAsync_ExistingClub_ReturnsHardwareSpecs()
    {
        var club = CreateTestClub();
        SetupClubs([club]);

        var result = await _clubService.GetClubDetailsAsync(club.Id);

        var pc01 = result.Value.Hardwares.First(h => h.PcNumber == "PC-01");
        pc01.Specs.Should().ContainKey("CPU");
        pc01.Specs["GPU"].Should().Be("RTX 4070");
    }

    [Fact]
    public async Task GetClubDetailsAsync_ExistingClub_ReturnsWorkHours()
    {
        var club = CreateTestClub();
        SetupClubs([club]);

        var result = await _clubService.GetClubDetailsAsync(club.Id);

        result.Value.WorkHours.Should().HaveCount(2);
        result.Value.WorkHours["Monday"].Should().Be("09:00-23:00");
    }

    // --- GetClubDetailsAsync Failure ---

    [Fact]
    public async Task GetClubDetailsAsync_NonExistentId_ReturnsFailure()
    {
        SetupClubs([]);

        var result = await _clubService.GetClubDetailsAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task GetClubDetailsAsync_NonExistentId_ValueAccessThrows()
    {
        SetupClubs([]);

        var result = await _clubService.GetClubDetailsAsync(Guid.NewGuid());

        var act = () => result.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    // --- GetClubsForCatalogAsync ---

    [Fact]
    public async Task GetClubsForCatalogAsync_ReturnsSuccessWithClubs()
    {
        var club = CreateTestClub();
        SetupClubs([club]);

        var result = await _clubService.GetClubsForCatalogAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetClubsForCatalogAsync_EmptyDb_ReturnsSuccessWithEmpty()
    {
        SetupClubs([]);

        var result = await _clubService.GetClubsForCatalogAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetClubsForCatalogAsync_ReturnsMinPrice()
    {
        var club = CreateTestClub();
        SetupClubs([club]);

        var result = await _clubService.GetClubsForCatalogAsync();

        result.Value.First().MinPrice.Should().Be(75m);
    }

    // --- GetClubDetailsAsync Reviews ---

    [Fact]
    public async Task GetClubDetailsAsync_ExistingClub_ReturnsReviews()
    {
        var club = CreateTestClub();
        var user = new User { Id = Guid.NewGuid(), UserName = "reviewer1", FullName = "Test Reviewer" };
        club.Reviews.Add(new Review
        {
            UserId = user.Id,
            User = user,
            ClubId = club.Id,
            Rating = 5,
            Comment = "Excellent club!",
            CreatedAt = DateTime.UtcNow
        });
        SetupClubs([club]);

        var result = await _clubService.GetClubDetailsAsync(club.Id);

        result.Value.Reviews.Should().HaveCount(1);
        result.Value.Reviews.First().UserName.Should().Be("reviewer1");
        result.Value.Reviews.First().Rating.Should().Be(5);
        result.Value.Reviews.First().Comment.Should().Be("Excellent club!");
    }

    [Fact]
    public async Task GetClubDetailsAsync_NoReviews_ReturnsEmptyList()
    {
        var club = CreateTestClub();
        SetupClubs([club]);

        var result = await _clubService.GetClubDetailsAsync(club.Id);

        result.Value.Reviews.Should().BeEmpty();
    }
}
