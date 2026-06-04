using CyberZone.Application.Common;
using CyberZone.Application.DTOs;

namespace CyberZone.Application.Interfaces;

public interface IBookingService
{
    /// <summary>
    /// Prepares the form model for /Booking/Create — tariffs, PC info, user balance.
    /// </summary>
    Task<Result<BookNowDto>> PrepareFormAsync(Guid userId, Guid clubId, Guid hardwareId);

    /// <summary>
    /// Validates the slot (PC available + not overlapping), charges the user balance,
    /// and creates a Pending Booking.
    /// </summary>
    Task<Result<Guid>> CreateAsync(Guid userId, BookNowDto dto);

    /// <summary>
    /// Cancels a booking owned by the given user. Only Pending or Confirmed bookings can be cancelled.
    /// </summary>
    Task<Result> CancelAsync(Guid bookingId, Guid userId, string? reason);
}
