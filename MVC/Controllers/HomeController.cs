using CyberZone.Application.Interfaces;
using CyberZone.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using System.Diagnostics;

namespace MVC.Controllers;

public class HomeController : Controller
{
    private readonly IClubService _clubService;
    private readonly IClubMapService _clubMapService;
    private readonly UserManager<User> _userManager;

    public HomeController(IClubService clubService, IClubMapService clubMapService, UserManager<User> userManager)
    {
        _clubService = clubService;
        _clubMapService = clubMapService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _clubService.GetClubsForCatalogAsync();
        return View(result.IsSuccess ? result.Value : Enumerable.Empty<CyberZone.Application.DTOs.ClubCatalogDto>());
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public async Task<IActionResult> Catalog()
    {
        var result = await _clubService.GetClubsForCatalogAsync();
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return View(Enumerable.Empty<CyberZone.Application.DTOs.ClubCatalogDto>());
        }

        return View(result.Value);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var result = await _clubService.GetClubDetailsAsync(id);
        if (result.IsFailure)
        {
            return NotFound();
        }

        return View(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> Map(Guid id)
    {
        var result = await _clubMapService.GetMapByClubIdAsync(id);
        if (result.IsFailure)
        {
            return NotFound();
        }

        return View(result.Value);
    }

    [Authorize(Roles = "Admin, Staff")]
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);
        bool isAdmin = await _userManager.IsInRoleAsync(user!, "Admin");
        
        if (!isAdmin && user?.ManagedClubId != id)
        {
            return Forbid();
        }

        var result = await _clubService.GetClubForEditAsync(id);
        if (result.IsFailure)
        {
            return NotFound();
        }

        return View(result.Value);
    }

    [Authorize(Roles = "Admin, Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CyberZone.Application.DTOs.EditClubDto dto)
    {
        if (id != dto.Id) return BadRequest();

        var user = await _userManager.GetUserAsync(User);
        bool isAdmin = await _userManager.IsInRoleAsync(user!, "Admin");

        if (!isAdmin && user?.ManagedClubId != id)
        {
            return Forbid();
        }

        if (!ModelState.IsValid) return View(dto);

        var result = await _clubService.UpdateClubAsync(id, dto);
        if (result.IsSuccess)
        {
            TempData["Success"] = "Дані клубу успішно оновлено.";
            return RedirectToAction(nameof(Details), new { id = id });
        }

        ModelState.AddModelError("", result.Error ?? "Error updating club");
        return View(dto);
    }

    [Authorize(Roles = "Admin, Staff")]
    [HttpGet]
    public async Task<IActionResult> CreateTariff(Guid clubId)
    {
        var user = await _userManager.GetUserAsync(User);
        bool isAdmin = await _userManager.IsInRoleAsync(user!, "Admin");

        if (!isAdmin && user?.ManagedClubId != clubId) return Forbid();

        return View(new CyberZone.Application.DTOs.CreateTariffDto { ClubId = clubId });
    }

    [Authorize(Roles = "Admin, Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTariff(CyberZone.Application.DTOs.CreateTariffDto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        bool isAdmin = await _userManager.IsInRoleAsync(user!, "Admin");

        if (!isAdmin && user?.ManagedClubId != dto.ClubId) return Forbid();

        if (!ModelState.IsValid) return View(dto);

        var result = await _clubService.AddTariffAsync(dto);
        if (result.IsSuccess)
            return RedirectToAction(nameof(Details), new { id = dto.ClubId });

        ModelState.AddModelError("", result.Error ?? "Помилка при створенні");
        return View(dto);
    }

    [Authorize(Roles = "Admin, Staff")]
    [HttpGet]
    public async Task<IActionResult> EditTariff(Guid id)
    {
        var result = await _clubService.GetTariffForEditAsync(id);
        if (result.IsFailure) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        bool isAdmin = await _userManager.IsInRoleAsync(user!, "Admin");

        if (!isAdmin && user?.ManagedClubId != result.Value.ClubId) return Forbid();

        return View(result.Value);
    }

    [Authorize(Roles = "Admin, Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTariff(Guid id, CyberZone.Application.DTOs.EditTariffDto dto)
    {
        if (id != dto.Id) return BadRequest();

        var user = await _userManager.GetUserAsync(User);
        bool isAdmin = await _userManager.IsInRoleAsync(user!, "Admin");

        if (!isAdmin && user?.ManagedClubId != dto.ClubId) return Forbid();

        if (!ModelState.IsValid) return View(dto);

        var result = await _clubService.UpdateTariffAsync(id, dto);
        if (result.IsSuccess)
            return RedirectToAction(nameof(Details), new { id = dto.ClubId });

        ModelState.AddModelError("", result.Error ?? "Помилка редагування");
        return View(dto);
    }

    [Authorize(Roles = "Admin, Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTariff(Guid id, Guid clubId)
    {
        var user = await _userManager.GetUserAsync(User);
        bool isAdmin = await _userManager.IsInRoleAsync(user!, "Admin");

        if (!isAdmin && user?.ManagedClubId != clubId) return Forbid();

        await _clubService.DeleteTariffAsync(id);
        return RedirectToAction(nameof(Details), new { id = clubId });
    }
}
