using CyberZone.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using System.Diagnostics;

namespace MVC.Controllers;

public class HomeController : Controller
{
    private readonly IClubService _clubService;

    public HomeController(IClubService clubService)
    {
        _clubService = clubService;
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
}
