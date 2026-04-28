
using System;
using System.IO;
using System.Text;
var code = @"
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
[Authorize(Roles = ""Admin, Staff"")]
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
        var user = await _userManager.GetUserAsync(User);
        return user?.ManagedClubId;
    }
    public async Task<IActionResult> Index()
    {
        var managedClubId = await GetManagedClubIdAsync();
        if (managedClubId == null) return Unauthorized(""You do not have a managed club assigned."");
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
        if (dto.Category == ""Снеки"") dto.Category = ""Snacks"";
        if (dto.Category == ""апої"") dto.Category = ""Drinks"";
        var result = await _barService.CreateMenuItemAsync(dto);
        if (result.IsFailure) { ModelState.AddModelError("""", result.Error ?? ""Unknown error occurred.""); return View(dto); }
        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Edit(Guid id)
    {
        var managedClubId = await GetManagedClubIdAsync();
        if (managedClubId == null) return Unauthorized();
        var result = await _barService.GetMenuItemAsync(id);
        if (result.IsFailure) return NotFound(result.Error);
        var dto = new UpdateBarMenuItemDto
        {
            Name = result.Value.Name,
            Description = result.Value.Description,
            Price = result.Value.Price,
            Category = result.Value.Category,
            IsAvailable = result.Value.IsAvailable,
            StockQuantity = result.Value.StockQuantity,
            IsActive = result.Value.IsActive,
            ImageUrl = result.Value.ImageUrl
        };
        return View(dto);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateBarMenuItemDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        if (dto.Category == ""Снеки"") dto.Category = ""Snacks"";
        if (dto.Category == ""апої"") dto.Category = ""Drinks"";
        var result = await _barService.UpdateMenuItemAsync(id, dto);
        if (result.IsFailure) { ModelState.AddModelError("""", result.Error ?? ""Unknown error.""); return View(dto); }
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
        if (managedClubId == null) return Unauthorized(""You do not have a managed club assigned."");
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
";
File.WriteAllText(""MVC/Controllers/BarAdminController.cs"", code, Encoding.UTF8);
