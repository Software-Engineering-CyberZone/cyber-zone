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
}
