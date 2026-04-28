using CyberZone.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MVC.Controllers;

public class DealsController : Controller
{
    private readonly IDealsService _dealsService;

    public DealsController(IDealsService dealsService)
    {
        _dealsService = dealsService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string sortBy = "Savings", int pageSize = 30, CancellationToken ct = default)
    {
        var result = await _dealsService.GetDealsAsync(pageSize, sortBy, ct);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return View(Array.Empty<CyberZone.Application.DTOs.GameDealDto>());
        }

        ViewData["SortBy"] = sortBy;
        return View(result.Value);
    }
}
