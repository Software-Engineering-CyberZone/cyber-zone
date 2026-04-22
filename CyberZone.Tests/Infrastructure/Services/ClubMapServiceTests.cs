using CyberZone.Application.Interfaces;
using CyberZone.Domain.Entities;
using CyberZone.Domain.Enums;
using CyberZone.Infrastructure.Services;
using CyberZone.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CyberZone.Tests.Infrastructure.Services;

public class ClubMapServiceTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly ClubMapService _service;

    public ClubMapServiceTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        var logger = new Mock<ILogger<ClubMapService>>();
        _service = new ClubMapService(_mockContext.Object, logger.Object, new NoOpCacheService(), CacheTestHelper.DefaultOptions());
    }

    private static (Club club, ClubMap map, Hardware hwAvailable, Hardware hwBusy) CreateTestMap()
    {
        var club = new Club { Name = "CyberPro Arena" };

        var hwAvailable = new Hardware
        {
            PcNumber = "1",
            ClubId = club.Id,
            Club = club,
            Status = HardwareStatus.Available,
            Specs = new Dictionary<string, string> { ["CPU"] = "i7" }
        };

        var hwBusy = new Hardware
        {
            PcNumber = "2",
            ClubId = club.Id,
            Club = club,
            Status = HardwareStatus.Busy,
            Specs = new Dictionary<string, string> { ["CPU"] = "i5" }
        };

        var standardZone = new ClubMapZone
        {
            Name = "Стандарт",
            X = 40, Y = 40, Width = 540, Height = 520,
            LabelColor = "yellow"
        };

        var proZone = new ClubMapZone
        {
            Name = "PC PRO",
            X = 700, Y = 40, Width = 260, Height = 520,
            LabelColor = "pink"
        };

        var pcEl1 = new ClubMapElement
        {
            ElementType = ClubMapElementType.Pc,
            X = 100, Y = 90, Width = 30, Height = 30,
            Label = "1",
            Zone = standardZone,
            Hardware = hwAvailable,
            HardwareId = hwAvailable.Id
        };

        var pcEl2 = new ClubMapElement
        {
            ElementType = ClubMapElementType.Pc,
            X = 140, Y = 90, Width = 30, Height = 30,
            Label = "2",
            Zone = standardZone,
            Hardware = hwBusy,
            HardwareId = hwBusy.Id
        };

        var barEl = new ClubMapElement
        {
            ElementType = ClubMapElementType.Bar,
            X = 440, Y = 70, Width = 120, Height = 30
        };

        var map = new ClubMap
        {
            ClubId = club.Id,
            Club = club,
            Width = 1000,
            Height = 600,
            BackgroundColor = "#0f1f2d"
        };
        map.Zones.Add(standardZone);
        map.Zones.Add(proZone);
        map.Elements.Add(pcEl1);
        map.Elements.Add(pcEl2);
        map.Elements.Add(barEl);

        club.Map = map;
        club.Hardwares.Add(hwAvailable);
        club.Hardwares.Add(hwBusy);

        return (club, map, hwAvailable, hwBusy);
    }

    private void SetupMap(List<ClubMap> maps, List<Tariff>? tariffs = null)
    {
        var mapSet = MockDbSetHelper.CreateMockDbSet(maps);
        _mockContext.Setup(c => c.ClubMaps).Returns(mapSet.Object);

        var tariffSet = MockDbSetHelper.CreateMockDbSet(tariffs ?? []);
        _mockContext.Setup(c => c.Tariffs).Returns(tariffSet.Object);
    }

    // --- Success ---

    [Fact]
    public async Task GetMapByClubIdAsync_ExistingMap_ReturnsSuccess()
    {
        var (_, map, _, _) = CreateTestMap();
        SetupMap([map]);

        var result = await _service.GetMapByClubIdAsync(map.ClubId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMapByClubIdAsync_ExistingMap_ReturnsBasicInfo()
    {
        var (club, map, _, _) = CreateTestMap();
        SetupMap([map]);

        var result = await _service.GetMapByClubIdAsync(map.ClubId);

        var dto = result.Value;
        dto.Id.Should().Be(map.Id);
        dto.ClubId.Should().Be(club.Id);
        dto.ClubName.Should().Be("CyberPro Arena");
        dto.Width.Should().Be(1000);
        dto.Height.Should().Be(600);
        dto.BackgroundColor.Should().Be("#0f1f2d");
    }

    [Fact]
    public async Task GetMapByClubIdAsync_ExistingMap_ReturnsZones()
    {
        var (_, map, _, _) = CreateTestMap();
        SetupMap([map]);

        var result = await _service.GetMapByClubIdAsync(map.ClubId);

        result.Value.Zones.Should().HaveCount(2);
        result.Value.Zones.Should().Contain(z => z.Name == "Стандарт" && z.LabelColor == "yellow");
        result.Value.Zones.Should().Contain(z => z.Name == "PC PRO" && z.LabelColor == "pink");
    }

    [Fact]
    public async Task GetMapByClubIdAsync_ExistingMap_ReturnsElements()
    {
        var (_, map, _, _) = CreateTestMap();
        SetupMap([map]);

        var result = await _service.GetMapByClubIdAsync(map.ClubId);

        result.Value.Elements.Should().HaveCount(3);
        result.Value.Elements.Should().Contain(e => e.ElementType == ClubMapElementType.Bar);
        result.Value.Elements.Count(e => e.ElementType == ClubMapElementType.Pc).Should().Be(2);
    }

    [Fact]
    public async Task GetMapByClubIdAsync_PcElement_IncludesHardwareStatusAndNumber()
    {
        var (_, map, hwAvailable, _) = CreateTestMap();
        SetupMap([map]);

        var result = await _service.GetMapByClubIdAsync(map.ClubId);

        var el = result.Value.Elements.First(e => e.HardwareId == hwAvailable.Id);
        el.PcNumber.Should().Be("1");
        el.HardwareStatus.Should().Be(HardwareStatus.Available);
    }

    [Fact]
    public async Task GetMapByClubIdAsync_BusyPc_ReturnsBusyStatus()
    {
        var (_, map, _, hwBusy) = CreateTestMap();
        SetupMap([map]);

        var result = await _service.GetMapByClubIdAsync(map.ClubId);

        var el = result.Value.Elements.First(e => e.HardwareId == hwBusy.Id);
        el.HardwareStatus.Should().Be(HardwareStatus.Busy);
    }

    [Fact]
    public async Task GetMapByClubIdAsync_NonPcElement_HasNoHardware()
    {
        var (_, map, _, _) = CreateTestMap();
        SetupMap([map]);

        var result = await _service.GetMapByClubIdAsync(map.ClubId);

        var bar = result.Value.Elements.First(e => e.ElementType == ClubMapElementType.Bar);
        bar.HardwareId.Should().BeNull();
        bar.PcNumber.Should().BeNull();
        bar.HardwareStatus.Should().BeNull();
    }

    [Fact]
    public async Task GetMapByClubIdAsync_ElementCoordinates_PreservedInDto()
    {
        var (_, map, _, _) = CreateTestMap();
        SetupMap([map]);

        var result = await _service.GetMapByClubIdAsync(map.ClubId);

        var bar = result.Value.Elements.First(e => e.ElementType == ClubMapElementType.Bar);
        bar.X.Should().Be(440);
        bar.Y.Should().Be(70);
        bar.Width.Should().Be(120);
        bar.Height.Should().Be(30);
    }

    // --- MinPricePerHour ---

    [Fact]
    public async Task GetMapByClubIdAsync_WithTariffs_ReturnsMinPricePerHourOnPcElements()
    {
        var (club, map, _, _) = CreateTestMap();
        var tariffs = new List<Tariff>
        {
            new() { ClubId = club.Id, Name = "Hourly", PricePerHour = 100m },
            new() { ClubId = club.Id, Name = "Night", PricePerHour = 75m }
        };
        SetupMap([map], tariffs);

        var result = await _service.GetMapByClubIdAsync(map.ClubId);

        var pcElements = result.Value.Elements.Where(e => e.ElementType == ClubMapElementType.Pc);
        pcElements.Should().OnlyContain(e => e.MinPricePerHour == 75m);
    }

    [Fact]
    public async Task GetMapByClubIdAsync_NoTariffs_ReturnsNullOrZeroPrice()
    {
        var (_, map, _, _) = CreateTestMap();
        SetupMap([map], tariffs: []);

        var result = await _service.GetMapByClubIdAsync(map.ClubId);

        var pcElements = result.Value.Elements.Where(e => e.ElementType == ClubMapElementType.Pc);
        pcElements.Should().OnlyContain(e => e.MinPricePerHour == null || e.MinPricePerHour == 0m);
    }

    [Fact]
    public async Task GetMapByClubIdAsync_TariffsFromOtherClub_DoNotAffectMinPrice()
    {
        var (club, map, _, _) = CreateTestMap();
        var otherClubId = Guid.NewGuid();
        var tariffs = new List<Tariff>
        {
            new() { ClubId = club.Id, Name = "Hourly", PricePerHour = 200m },
            new() { ClubId = otherClubId, Name = "Cheap", PricePerHour = 10m }
        };
        SetupMap([map], tariffs);

        var result = await _service.GetMapByClubIdAsync(map.ClubId);

        var pc = result.Value.Elements.First(e => e.ElementType == ClubMapElementType.Pc);
        pc.MinPricePerHour.Should().Be(200m);
    }

    // --- Failure ---

    [Fact]
    public async Task GetMapByClubIdAsync_NonExistentClub_ReturnsFailure()
    {
        SetupMap([]);

        var result = await _service.GetMapByClubIdAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task GetMapByClubIdAsync_NonExistentClub_ValueAccessThrows()
    {
        SetupMap([]);

        var result = await _service.GetMapByClubIdAsync(Guid.NewGuid());

        var act = () => result.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task GetMapByClubIdAsync_DifferentClubId_ReturnsFailure()
    {
        var (_, map, _, _) = CreateTestMap();
        SetupMap([map]);

        var result = await _service.GetMapByClubIdAsync(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
    }
}
