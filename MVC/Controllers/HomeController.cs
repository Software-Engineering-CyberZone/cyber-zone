using CyberZone.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Models;
using System.Diagnostics;

namespace MVC.Controllers;

public class HomeController : Controller
{
    private readonly IApplicationDbContext _context;
    private readonly IClubService _clubService;

    public HomeController(IApplicationDbContext context, IClubService clubService)
    {
        _context = context;
        _clubService = clubService;
    }

    public async Task<IActionResult> Index()
    {
        var clubs = await _context.Clubs.ToListAsync();
        return View(clubs);
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

    [Authorize]
    public async Task<IActionResult> Catalog()
    {
        var clubs = await _clubService.GetClubsForCatalogAsync();
        return View(clubs); // ѕередаЇмо список у View
    }
}