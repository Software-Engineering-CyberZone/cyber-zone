using System.Security.Claims;
using CyberZone.Application.DTOs;
using CyberZone.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC.Filters;

namespace MVC.Controllers;

[Authorize]
public class BookingController : Controller
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    public async Task<IActionResult> Create(Guid clubId, Guid hardwareId)
    {
        var userId = GetUserId();
        if (userId is null) return RedirectToAction("Login", "Account");

        var result = await _bookingService.PrepareFormAsync(userId.Value, clubId, hardwareId);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction("Map", "Home", new { id = clubId });
        }

        return View(result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RateLimit(5)]  // max 5 booking attempts per minute per IP
    public async Task<IActionResult> Create(BookNowDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
        {
            var reload = await _bookingService.PrepareFormAsync(userId.Value, dto.ClubId, dto.HardwareId);
            if (reload.IsSuccess)
            {
                reload.Value.TariffId = dto.TariffId;
                reload.Value.StartTime = dto.StartTime;
                reload.Value.Hours = dto.Hours;
                reload.Value.Notes = dto.Notes;
                return View(reload.Value);
            }
            return RedirectToAction("Map", "Home", new { id = dto.ClubId });
        }

        var result = await _bookingService.CreateAsync(userId.Value, dto);
        if (result.IsFailure)
        {
            ModelState.AddModelError("", result.Error ?? "Не вдалося створити бронювання.");
            var reload = await _bookingService.PrepareFormAsync(userId.Value, dto.ClubId, dto.HardwareId);
            return View(reload.IsSuccess ? reload.Value : dto);
        }

        TempData["Success"] = "Бронювання успішно створено!";
        return RedirectToAction("Sessions", "Account");
    }

    private Guid? GetUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out var id) ? id : null;
    }
}
