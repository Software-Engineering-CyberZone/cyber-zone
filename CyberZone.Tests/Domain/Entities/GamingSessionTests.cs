using CyberZone.Domain.Entities;
using CyberZone.Domain.Enums;
using FluentAssertions;

namespace CyberZone.Tests.Domain.Entities;

public class GamingSessionTests
{
    private GamingSession CreateSession(SessionStatus status = SessionStatus.Active, decimal pricePerHour = 100m)
    {
        return new GamingSession
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            HardwareId = Guid.NewGuid(),
            TariffId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddHours(-2),
            Status = status,
            Tariff = new Tariff
            {
                Id = Guid.NewGuid(),
                Name = "Standard",
                PricePerHour = pricePerHour,
                ClubId = Guid.NewGuid()
            }
        };
    }

    [Fact]
    public void EndSession_WhenActive_SetsStatusToCompleted()
    {
        var session = CreateSession();

        session.EndSession();

        session.Status.Should().Be(SessionStatus.Completed);
    }

    [Fact]
    public void EndSession_WhenActive_SetsEndTime()
    {
        var session = CreateSession();

        session.EndSession();

        session.EndTime.Should().NotBeNull();
        session.EndTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void EndSession_CalculatesTotalCostBasedOnDuration()
    {
        var session = CreateSession(pricePerHour: 150m);
        session.StartTime = DateTime.UtcNow.AddHours(-3);

        session.EndSession();

        // ~3 hours * 150/hour = ~450
        session.TotalCost.Should().BeApproximately(450m, 1m);
    }

    [Fact]
    public void EndSession_TotalCost_IsRoundedToTwoDecimalPlaces()
    {
        var session = CreateSession(pricePerHour: 100m);
        // Set a start time that will produce a non-round number of hours
        session.StartTime = DateTime.UtcNow.AddMinutes(-90);

        session.EndSession();

        // 1.5 hours * 100 = 150.00
        var decimalPlaces = BitConverter.GetBytes(decimal.GetBits(session.TotalCost)[3])[2];
        decimalPlaces.Should().BeLessThanOrEqualTo(2);
    }

    [Theory]
    [InlineData(SessionStatus.Paused)]
    [InlineData(SessionStatus.Completed)]
    [InlineData(SessionStatus.Terminated)]
    public void EndSession_WhenNotActive_ThrowsInvalidOperationException(SessionStatus status)
    {
        var session = CreateSession(status);

        var act = () => session.EndSession();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*active*");
    }

    [Fact]
    public void DefaultStatus_IsActive()
    {
        var session = new GamingSession();

        session.Status.Should().Be(SessionStatus.Active);
    }

    [Fact]
    public void EndSession_ShortSession_CalculatesCorrectCost()
    {
        var session = CreateSession(pricePerHour: 200m);
        session.StartTime = DateTime.UtcNow.AddMinutes(-30);

        session.EndSession();

        // ~0.5 hours * 200 = ~100
        session.TotalCost.Should().BeApproximately(100m, 1m);
    }
}
