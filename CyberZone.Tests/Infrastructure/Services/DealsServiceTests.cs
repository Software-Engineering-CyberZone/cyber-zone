using CyberZone.Infrastructure.ExternalApis.CheapShark;
using CyberZone.Infrastructure.Services;
using CyberZone.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CyberZone.Tests.Infrastructure.Services;

public class DealsServiceTests
{
    private readonly Mock<ICheapSharkApi> _mockApi;
    private readonly DealsService _service;

    public DealsServiceTests()
    {
        _mockApi = new Mock<ICheapSharkApi>();
        var logger = new Mock<ILogger<DealsService>>();
        _service = new DealsService(_mockApi.Object, new NoOpCacheService(), CacheTestHelper.DefaultOptions(), logger.Object);
    }

    private static CheapSharkDealResponse Raw(
        string title = "Cyberpunk 2077",
        string sale = "29.99",
        string normal = "59.99",
        string savings = "50.01",
        string storeId = "1",
        string? thumb = "https://example.com/thumb.jpg",
        string? metacritic = "86") => new()
    {
        Title = title,
        SalePrice = sale,
        NormalPrice = normal,
        Savings = savings,
        StoreId = storeId,
        Thumb = thumb,
        MetacriticScore = metacritic,
        DealId = Guid.NewGuid().ToString("N"),
        SteamAppId = "1091500"
    };

    // --- Success mapping ---

    [Fact]
    public async Task GetDealsAsync_ValidResponse_ReturnsSuccess()
    {
        _mockApi.Setup(a => a.GetDealsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<decimal?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CheapSharkDealResponse> { Raw() });

        var result = await _service.GetDealsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetDealsAsync_MapsPricesAndSavingsCorrectly()
    {
        _mockApi.Setup(a => a.GetDealsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<decimal?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CheapSharkDealResponse> { Raw(sale: "9.99", normal: "39.99", savings: "75.02") });

        var result = await _service.GetDealsAsync();

        var d = result.Value[0];
        d.SalePrice.Should().Be(9.99m);
        d.NormalPrice.Should().Be(39.99m);
        d.SavingsPercent.Should().Be(75m);
    }

    [Fact]
    public async Task GetDealsAsync_MapsKnownStoreIdToFriendlyName()
    {
        _mockApi.Setup(a => a.GetDealsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<decimal?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CheapSharkDealResponse> { Raw(storeId: "1"), Raw(storeId: "7") });

        var result = await _service.GetDealsAsync();

        result.Value[0].StoreName.Should().Be("Steam");
        result.Value[1].StoreName.Should().Be("GOG");
    }

    [Fact]
    public async Task GetDealsAsync_UnknownStoreId_FallsBackToNumericLabel()
    {
        _mockApi.Setup(a => a.GetDealsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<decimal?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CheapSharkDealResponse> { Raw(storeId: "999") });

        var result = await _service.GetDealsAsync();

        result.Value[0].StoreName.Should().Be("Store 999");
    }

    [Fact]
    public async Task GetDealsAsync_MetacriticZero_ReturnsNull()
    {
        _mockApi.Setup(a => a.GetDealsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<decimal?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CheapSharkDealResponse> { Raw(metacritic: "0") });

        var result = await _service.GetDealsAsync();

        result.Value[0].MetacriticScore.Should().BeNull();
    }

    [Fact]
    public async Task GetDealsAsync_MetacriticNonZero_ParsedCorrectly()
    {
        _mockApi.Setup(a => a.GetDealsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<decimal?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CheapSharkDealResponse> { Raw(metacritic: "92") });

        var result = await _service.GetDealsAsync();

        result.Value[0].MetacriticScore.Should().Be(92);
    }

    [Fact]
    public async Task GetDealsAsync_EmptyOrWhitespaceTitle_Filtered()
    {
        _mockApi.Setup(a => a.GetDealsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<decimal?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CheapSharkDealResponse>
            {
                Raw(),
                Raw(title: ""),
                Raw(title: "   ")
            });

        var result = await _service.GetDealsAsync();

        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetDealsAsync_PassesThroughPageSizeAndSort()
    {
        _mockApi.Setup(a => a.GetDealsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<decimal?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CheapSharkDealResponse>());

        await _service.GetDealsAsync(pageSize: 15, sortBy: "Metacritic");

        _mockApi.Verify(a => a.GetDealsAsync(
            It.IsAny<int>(),
            15,
            "Metacritic",
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<decimal?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- Resilience / failure path ---

    [Fact]
    public async Task GetDealsAsync_UpstreamThrows_ReturnsFailure()
    {
        _mockApi.Setup(a => a.GetDealsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<decimal?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("CheapShark is down"));

        var result = await _service.GetDealsAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("недоступні");
    }

    [Fact]
    public async Task GetDealsAsync_UpstreamThrows_DoesNotBubbleUp()
    {
        _mockApi.Setup(a => a.GetDealsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<decimal?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException());

        var act = async () => await _service.GetDealsAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetDealsAsync_EmptyResponse_ReturnsSuccessWithEmptyList()
    {
        _mockApi.Setup(a => a.GetDealsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<decimal?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CheapSharkDealResponse>());

        var result = await _service.GetDealsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // --- Caching ---

    [Fact]
    public async Task GetDealsAsync_WithRealCache_HitsApiOnlyOnce()
    {
        var cache = new StubCache();
        var logger = new Mock<ILogger<DealsService>>();
        var service = new DealsService(_mockApi.Object, cache, CacheTestHelper.DefaultOptions(), logger.Object);

        _mockApi.Setup(a => a.GetDealsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<decimal?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CheapSharkDealResponse> { Raw() });

        await service.GetDealsAsync();
        await service.GetDealsAsync();
        await service.GetDealsAsync();

        _mockApi.Verify(a => a.GetDealsAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<decimal?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // --- AAA highlights ---

    [Fact]
    public async Task GetAaaHighlightsAsync_PassesAaaFlagAndRecentSort()
    {
        _mockApi.Setup(a => a.GetDealsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<decimal?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CheapSharkDealResponse>());

        await _service.GetAaaHighlightsAsync(pageSize: 6);

        _mockApi.Verify(a => a.GetDealsAsync(
            It.IsAny<int>(),
            6,
            "Recent",
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            "1",
            It.IsAny<decimal?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAaaHighlightsAsync_UsesDifferentCacheKeyThanRegularDeals()
    {
        var cache = new StubCache();
        var logger = new Mock<ILogger<DealsService>>();
        var service = new DealsService(_mockApi.Object, cache, CacheTestHelper.DefaultOptions(), logger.Object);

        _mockApi.Setup(a => a.GetDealsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<decimal?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CheapSharkDealResponse> { Raw() });

        await service.GetDealsAsync(pageSize: 8);
        await service.GetAaaHighlightsAsync(pageSize: 8);

        // both keys should have hit the API
        _mockApi.Verify(a => a.GetDealsAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<decimal?>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task GetAaaHighlightsAsync_UpstreamThrows_ReturnsFailure()
    {
        _mockApi.Setup(a => a.GetDealsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<decimal?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException());

        var result = await _service.GetAaaHighlightsAsync();

        result.IsFailure.Should().BeTrue();
    }

    private class StubCache : CyberZone.Application.Interfaces.ICacheService
    {
        private readonly Dictionary<string, object> _store = new();

        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan ttl) where T : class
        {
            if (_store.TryGetValue(key, out var cached)) return (T)cached;
            var value = await factory();
            if (value is not null) _store[key] = value;
            return value;
        }

        public void Remove(string key) => _store.Remove(key);
        public void RemoveByPrefix(string prefix)
        {
            foreach (var k in _store.Keys.Where(k => k.StartsWith(prefix)).ToList()) _store.Remove(k);
        }
    }
}
