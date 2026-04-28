using CyberZone.Application.DTOs;
using CyberZone.Domain.Entities;
using CyberZone.Domain.Enums;
using CyberZone.Domain.ValueObjects;
using CyberZone.Tests.Helpers;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace CyberZone.Tests.Infrastructure.Services;

public class ClubServiceAdminActionsTests
{
    private readonly Mock<CyberZone.Application.Interfaces.IApplicationDbContext> _mockContext;
    private readonly ClubService _clubService;

    public ClubServiceAdminActionsTests()
    {
        _mockContext = new Mock<CyberZone.Application.Interfaces.IApplicationDbContext>(); 
        var logger = new Mock<ILogger<ClubService>>();
<<<<<<< Updated upstream
        _clubService = new ClubService(_mockContext.Object, logger.Object, new NoOpCacheService(), CacheTestHelper.DefaultOptions());
    }

    [Fact]
=======
        var cache = new Mock<CyberZone.Application.Interfaces.ICacheService>();
        var cacheOptions = Microsoft.Extensions.Options.Options.Create(new CyberZone.Application.Common.CacheOptions());
        _clubService = new ClubService(_mockContext.Object, logger.Object, cache.Object, cacheOptions);
    }    [Fact]
>>>>>>> Stashed changes
    public async Task GetClubForEditAsync_ExistingClub_ReturnsDto()
    {
        var clubId = Guid.NewGuid();
        var club = new Club
        {
            Id = clubId,
            Name = "Edit Club",
            Address = new Address { Street = "S", City = "C" },
            Description = "Desc"
        };

        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<Club> { club });
        _mockContext.Setup(c => c.Clubs).Returns(mockSet.Object);

        var result = await _clubService.GetClubForEditAsync(clubId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Edit Club");
        result.Value.Description.Should().Be("Desc");
    }

    [Fact]
    public async Task UpdateClubAsync_ExistingClub_UpdatesFields()
    {
        var clubId = Guid.NewGuid();
        var club = new Club
        {
            Id = clubId,
            Name = "Old Name"
        };
        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<Club> { club });
        _mockContext.Setup(c => c.Clubs).Returns(mockSet.Object);

        var dto = new EditClubDto
        {
            Id = clubId,
            Name = "New Name",
            Description = "New Desc",
            City = "NC",
            Street = "NS"
        };

        var result = await _clubService.UpdateClubAsync(clubId, dto);

        result.IsSuccess.Should().BeTrue();
        club.Name.Should().Be("New Name");
        club.Description.Should().Be("New Desc");
        club.Address.City.Should().Be("NC");
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddTariffAsync_ExistingClub_AddsTariff()
    {
        var clubId = Guid.NewGuid();
        var club = new Club { Id = clubId };
        
        var mockClubs = MockDbSetHelper.CreateMockDbSet(new List<Club> { club });
        // Assuming FindAsync works if mocked, or we setup find
        mockClubs.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync((object[] ids) => 
            (Guid)ids[0] == clubId ? club : null);

        _mockContext.Setup(c => c.Clubs).Returns(mockClubs.Object);

        var mockTariffs = MockDbSetHelper.CreateMockDbSet(new List<Tariff>());
        _mockContext.Setup(c => c.Tariffs).Returns(mockTariffs.Object);

        var dto = new CreateTariffDto
        {
            ClubId = clubId,
            Name = "New Tariff",
            PricePerHour = 100,
            Type = TariffType.Hourly
        };

        var result = await _clubService.AddTariffAsync(dto);

        result.IsSuccess.Should().BeTrue();
        mockTariffs.Verify(m => m.Add(It.IsAny<Tariff>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTariffAsync_ExistingTariff_RemovesTariff()
    {
        var tariffId = Guid.NewGuid();
        var tariff = new Tariff { Id = tariffId, ClubId = Guid.NewGuid() };
        
        var mockTariffs = MockDbSetHelper.CreateMockDbSet(new List<Tariff> { tariff });
        mockTariffs.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync((object[] ids) => 
            (Guid)ids[0] == tariffId ? tariff : null);

        _mockContext.Setup(c => c.Tariffs).Returns(mockTariffs.Object);

        var result = await _clubService.DeleteTariffAsync(tariffId);

        result.IsSuccess.Should().BeTrue();
        mockTariffs.Verify(m => m.Remove(tariff), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetClubForEditAsync_NonExistentClub_ReturnsFailure()
    {
        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<Club>());
        _mockContext.Setup(c => c.Clubs).Returns(mockSet.Object);

        var result = await _clubService.GetClubForEditAsync(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task UpdateClubAsync_NonExistentClub_ReturnsFailure()
    {
        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<Club>());
        _mockContext.Setup(c => c.Clubs).Returns(mockSet.Object);

        var result = await _clubService.UpdateClubAsync(Guid.NewGuid(), new EditClubDto());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddTariffAsync_NonExistentClub_ReturnsFailure()
    {
        var mockClubs = MockDbSetHelper.CreateMockDbSet(new List<Club>());
        mockClubs.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync((Club?)null);
        _mockContext.Setup(c => c.Clubs).Returns(mockClubs.Object);

        var mockTariffs = MockDbSetHelper.CreateMockDbSet(new List<Tariff>());
        _mockContext.Setup(c => c.Tariffs).Returns(mockTariffs.Object);

        var result = await _clubService.AddTariffAsync(new CreateTariffDto { ClubId = Guid.NewGuid() });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Club not found");
        mockTariffs.Verify(m => m.Add(It.IsAny<Tariff>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetTariffForEditAsync_ExistingTariff_ReturnsDto()
    {
        var tariffId = Guid.NewGuid();
        var tariff = new Tariff
        {
            Id = tariffId,
            ClubId = Guid.NewGuid(),
            Name = "VIP",
            PricePerHour = 150,
            Type = TariffType.Night,
            Description = "VIP Night"
        };
        
        var mockTariffs = MockDbSetHelper.CreateMockDbSet(new List<Tariff> { tariff });
        mockTariffs.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync((object[] ids) => 
            (Guid)ids[0] == tariffId ? tariff : null);

        _mockContext.Setup(c => c.Tariffs).Returns(mockTariffs.Object);

        var result = await _clubService.GetTariffForEditAsync(tariffId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("VIP");
        result.Value.PricePerHour.Should().Be(150);
        result.Value.Type.Should().Be(TariffType.Night);
    }

    [Fact]
    public async Task GetTariffForEditAsync_NonExistentTariff_ReturnsFailure()
    {
        var mockTariffs = MockDbSetHelper.CreateMockDbSet(new List<Tariff>());
        mockTariffs.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync((Tariff?)null);
        _mockContext.Setup(c => c.Tariffs).Returns(mockTariffs.Object);

        var result = await _clubService.GetTariffForEditAsync(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Tariff not found");
    }

    [Fact]
    public async Task UpdateTariffAsync_ExistingTariff_UpdatesFields()
    {
        var tariffId = Guid.NewGuid();
        var tariff = new Tariff { Id = tariffId, Name = "Old Name" };
        
        var mockTariffs = MockDbSetHelper.CreateMockDbSet(new List<Tariff> { tariff });
        mockTariffs.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync((object[] ids) => 
            (Guid)ids[0] == tariffId ? tariff : null);

        _mockContext.Setup(c => c.Tariffs).Returns(mockTariffs.Object);

        var dto = new EditTariffDto
        {
            Id = tariffId,
            Name = "New Name",
            PricePerHour = 200,
            Type = TariffType.Night
        };

        var result = await _clubService.UpdateTariffAsync(tariffId, dto);

        result.IsSuccess.Should().BeTrue();
        tariff.Name.Should().Be("New Name");
        tariff.PricePerHour.Should().Be(200);
        tariff.Type.Should().Be(TariffType.Night);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTariffAsync_NonExistentTariff_ReturnsFailure()
    {
        var mockTariffs = MockDbSetHelper.CreateMockDbSet(new List<Tariff>());
        mockTariffs.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync((Tariff?)null);
        _mockContext.Setup(c => c.Tariffs).Returns(mockTariffs.Object);

        var result = await _clubService.UpdateTariffAsync(Guid.NewGuid(), new EditTariffDto());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Tariff not found");
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTariffAsync_NonExistentTariff_ReturnsFailure()
    {
        var mockTariffs = MockDbSetHelper.CreateMockDbSet(new List<Tariff>());
        mockTariffs.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync((Tariff?)null);
        _mockContext.Setup(c => c.Tariffs).Returns(mockTariffs.Object);

        var result = await _clubService.DeleteTariffAsync(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Tariff not found");
        mockTariffs.Verify(m => m.Remove(It.IsAny<Tariff>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
