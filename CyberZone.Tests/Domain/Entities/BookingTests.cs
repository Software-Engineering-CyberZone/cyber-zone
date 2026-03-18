using CyberZone.Domain.Entities;
using CyberZone.Domain.Enums;
using FluentAssertions;

namespace CyberZone.Tests.Domain.Entities;

public class BookingTests
{
    private Booking CreateBooking(BookingStatus status = BookingStatus.Confirmed)
    {
        return new Booking
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            HardwareId = Guid.NewGuid(),
            TariffId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(2),
            Status = status
        };
    }

    [Fact]
    public void TransitionToSession_WhenConfirmed_ReturnsGamingSession()
    {
        var booking = CreateBooking(BookingStatus.Confirmed);

        var session = booking.TransitionToSession();

        session.Should().NotBeNull();
        session.UserId.Should().Be(booking.UserId);
        session.HardwareId.Should().Be(booking.HardwareId);
        session.TariffId.Should().Be(booking.TariffId);
        session.Status.Should().Be(SessionStatus.Active);
    }

    [Fact]
    public void TransitionToSession_WhenConfirmed_SetsBookingStatusToActive()
    {
        var booking = CreateBooking(BookingStatus.Confirmed);

        booking.TransitionToSession();

        booking.Status.Should().Be(BookingStatus.Active);
    }

    [Theory]
    [InlineData(BookingStatus.Pending)]
    [InlineData(BookingStatus.Active)]
    [InlineData(BookingStatus.Completed)]
    [InlineData(BookingStatus.Cancelled)]
    public void TransitionToSession_WhenNotConfirmed_ThrowsInvalidOperationException(BookingStatus status)
    {
        var booking = CreateBooking(status);

        var act = () => booking.TransitionToSession();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*confirmed*");
    }

    [Fact]
    public void TransitionToSession_SessionStartTime_IsSetToUtcNow()
    {
        var booking = CreateBooking(BookingStatus.Confirmed);
        var before = DateTime.UtcNow;

        var session = booking.TransitionToSession();

        session.StartTime.Should().BeOnOrAfter(before);
        session.StartTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void DefaultStatus_IsPending()
    {
        var booking = new Booking();

        booking.Status.Should().Be(BookingStatus.Pending);
    }
}
