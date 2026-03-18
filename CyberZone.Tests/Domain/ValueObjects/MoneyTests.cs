using CyberZone.Domain.ValueObjects;
using FluentAssertions;

namespace CyberZone.Tests.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_WithValidAmount_CreatesMoney()
    {
        var money = new Money(100m, "USD");

        money.Amount.Should().Be(100m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ThrowsArgumentException()
    {
        var act = () => new Money(-1m, "USD");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("amount");
    }

    [Fact]
    public void Constructor_WithZeroAmount_Succeeds()
    {
        var money = new Money(0m, "USD");

        money.Amount.Should().Be(0m);
    }

    [Fact]
    public void Zero_ReturnsMoneyWithZeroAmount()
    {
        var money = Money.Zero("EUR");

        money.Amount.Should().Be(0m);
        money.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Zero_DefaultCurrency_IsUSD()
    {
        var money = Money.Zero();

        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_SameCurrency_ReturnsSummedAmount()
    {
        var a = new Money(50m, "USD");
        var b = new Money(30m, "USD");

        var result = a.Add(b);

        result.Amount.Should().Be(80m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_DifferentCurrency_ThrowsInvalidOperationException()
    {
        var usd = new Money(50m, "USD");
        var eur = new Money(30m, "EUR");

        var act = () => usd.Add(eur);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*different currencies*");
    }

    [Fact]
    public void Subtract_SameCurrency_ReturnsSubtractedAmount()
    {
        var a = new Money(50m, "USD");
        var b = new Money(30m, "USD");

        var result = a.Subtract(b);

        result.Amount.Should().Be(20m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Subtract_DifferentCurrency_ThrowsInvalidOperationException()
    {
        var usd = new Money(50m, "USD");
        var eur = new Money(30m, "EUR");

        var act = () => usd.Subtract(eur);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*different currencies*");
    }

    [Fact]
    public void Subtract_ResultingInNegative_ThrowsArgumentException()
    {
        var a = new Money(10m, "USD");
        var b = new Money(50m, "USD");

        var act = () => a.Subtract(b);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equals_SameValues_AreEqual()
    {
        var a = new Money(100m, "USD");
        var b = new Money(100m, "USD");

        a.Should().Be(b);
    }

    [Fact]
    public void Equals_DifferentAmounts_AreNotEqual()
    {
        var a = new Money(100m, "USD");
        var b = new Money(200m, "USD");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Equals_DifferentCurrencies_AreNotEqual()
    {
        var a = new Money(100m, "USD");
        var b = new Money(100m, "EUR");

        a.Should().NotBe(b);
    }
}
