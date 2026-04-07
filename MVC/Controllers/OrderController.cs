using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Models;
using CyberZone.Infrastructure.Persistence;

namespace MVC.Controllers;

[Authorize]
public class OrderController : Controller
{
    private readonly CyberZoneDbContext _context;

    public OrderController(CyberZoneDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Bar() // <-- ÏÐÈÁÐÀËÈ sessionId
    {
        // 1. Ä³ñòàºìî âñ³ ÄÎÑÒÓÏÍ² òîâàðè ç áàçè
        var items = await _context.MenuItems
            .Where(m => m.IsAvailable)
            .ToListAsync();

        // 2. Ãðóïóºìî ¿õ çà êàòåãîð³ÿìè
        var model = new BarViewModel
        {
            Drinks = items.Where(i => i.Category == "Drinks").ToList(),
            Snacks = items.Where(i => i.Category == "Snacks").ToList()
        };

        return View(model);
    }
}