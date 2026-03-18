using CyberZone.Domain.ValueObjects;
using FluentAssertions;

namespace CyberZone.Tests.Domain.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Constructor_WithParameters_SetsAllProperties()
    {
        var address = new Address("123 Main St", "Kyiv", "Kyiv Oblast", "01001", "Ukraine");

        address.Street.Should().Be("123 Main St");
        address.City.Should().Be("Kyiv");
        address.State.Should().Be("Kyiv Oblast");
        address.ZipCode.Should().Be("01001");
        address.Country.Should().Be("Ukraine");
    }

    [Fact]
    public void ToString_ReturnsFormattedAddress()
    {
        var address = new Address("123 Main St", "Kyiv", "Kyiv Oblast", "01001", "Ukraine");

        address.ToString().Should().Be("123 Main St, Kyiv, Kyiv Oblast 01001, Ukraine");
    }

    [Fact]
    public void DefaultConstructor_InitializesWithEmptyStrings()
    {
        var address = new Address();

        address.Street.Should().BeEmpty();
        address.City.Should().BeEmpty();
        address.State.Should().BeEmpty();
        address.ZipCode.Should().BeEmpty();
        address.Country.Should().BeEmpty();
    }

    [Fact]
    public void Equals_SameValues_AreEqual()
    {
        var a = new Address("St", "City", "State", "00000", "Country");
        var b = new Address("St", "City", "State", "00000", "Country");

        a.Should().Be(b);
    }

    [Fact]
    public void Equals_DifferentValues_AreNotEqual()
    {
        var a = new Address("St A", "City", "State", "00000", "Country");
        var b = new Address("St B", "City", "State", "00000", "Country");

        a.Should().NotBe(b);
    }
}
