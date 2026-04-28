using System;
using System.Linq;
using System.Threading.Tasks;
using CyberZone.Application.DTOs;
using CyberZone.Application.Interfaces;
using CyberZone.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MVC.Controllers;

[Authorize(Roles = "Admin, Staff")]
public class BarAdminController : Controller
{
    private readonly IBarService _barService;
    private readonly UserManager<User> _userManager;

    public BarAdminController(IBarService barService, UserManager<User> userManager)
    {
        _barService = barService;
        _userManager = userManager;
    }

    private async Task<Guid?> GetManagedClubIdAsync()
    {
        // For Staff roles, we usually want to restrict to their managed club
        var user = await _userManager.GetUserAsync(User);
        return user?.ManagedClubId;
    }

    public async Task<IActionResult> Index()
    {
        var managedClubId = await GetManagedClubIdAsync();
        if (managedClubId == null)
            return Unauthorized("You do not have a managed club assigned.");

        var result = await _barService.GetMenuItemsAsync(managedClubId.Value);
        if (result.IsFailure) return NotFound(result.Error);

        return View(result.Value);
    }

    public async Task<IActionResult> Create()
    {
        var managedClubId = await GetManagedClubIdAsync();
        if (managedClubId == null) return Unauthorized();

        var dto = new CreateBarMenuItemDto { ClubId = managedClubId.Value };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBarMenuItemDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var result = await _barService.CreateMenuItemAsync(dto);
        if (result.IsFailure)
        {
            ModelState.AddModelError("", result.Error ?? "Unknown error occurred.");
            return View(dto);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStock(Guid id, int stockQuantity)
    {
        var result = await _barService.UpdateStockAsync(id, stockQuantity);
        if (result.IsFailure) return BadRequest(result.Error);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(Guid id, bool isActive)
    {
        var result = await _barService.ToggleActiveAsync(id, isActive);
        if (result.IsFailure) return BadRequest(result.Error);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Orders()
    {
        var managedClubId = await GetManagedClubIdAsync();
        if (managedClubId == null)
            return Unauthorized("You do not have a managed club assigned.");

        var result = await _barService.GetActiveOrdersAsync(managedClubId.Value);
        if (result.IsFailure) return NotFound(result.Error);

        return View(result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteOrder(Guid id)
    {
        var result = await _barService.CompleteOrderAsync(id);
        if (result.IsFailure) return BadRequest(result.Error);

        return RedirectToAction(nameof(Orders));
    }
}
