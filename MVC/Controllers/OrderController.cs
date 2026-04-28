using CyberZone.Domain.Entities;
using CyberZone.Domain.Enums;
using CyberZone.Infrastructure.Persistence;
using CyberZone.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Models;
using System.Security.Claims;

namespace MVC.Controllers;

public class CheckoutRequest
{
    public List<CheckoutItem> Items { get; set; } = new();
}

public class CheckoutItem
{
    public Guid MenuItemId { get; set; }
    public int Quantity { get; set; }
}

[Authorize]
public class OrderController : Controller
{
    private readonly CyberZoneDbContext _context;
    private readonly PaymentService _paymentService;

    public OrderController(CyberZoneDbContext context, PaymentService paymentService)
    {
        _context = context;
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<IActionResult> Bar() // Враховано поточний клуб сесії
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var activeSession = await _context.GamingSessions
            .Include(s => s.Hardware)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SessionStatus.Active);

        if (activeSession == null)
        {
            ViewBag.ErrorMessage = "У вас немає активної ігрової сесії. Замовлення з бару доступні лише під час гри за комп'ютером клубу.";
            return View(new BarViewModel { Drinks = new(), Snacks = new() });
        }

        // 1. Дістаємо всі ДОСТУПНІ товари з бази ТІЛЬКИ ДЛЯ ПОТОЧНОГО КЛУБУ і в наявності  
        var items = await _context.MenuItems
            .Where(m => m.IsAvailable && m.IsActive && m.StockQuantity > 0 && m.ClubId == activeSession.Hardware.ClubId)
            .ToListAsync();

        // 2. Групуємо їх за категоріями
        var model = new BarViewModel
        {
            Drinks = items.Where(i => i.Category == "Drinks" || i.Category == "Напої").ToList(),
            Snacks = items.Where(i => i.Category == "Snacks" || i.Category == "Снеки").ToList()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
    {
        if (request?.Items == null || !request.Items.Any())
            return BadRequest(new { success = false, message = "Кошик порожній" });

        // Отримуємо ID поточного користувача
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var activeSession = await _context.GamingSessions
            .Include(s => s.Hardware)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SessionStatus.Active);

        if (activeSession == null)
            return BadRequest(new { success = false, message = "Немає активної ігрової сесії." });

        // 1. Дістаємо актуальні товари з бази для формування цін (лише активні та цього клубу)
        var itemIds = request.Items.Select(i => i.MenuItemId).ToList();
        var menuItems = await _context.MenuItems
            .Where(m => itemIds.Contains(m.Id) && m.IsAvailable && m.IsActive && m.ClubId == activeSession.Hardware.ClubId)
            .ToDictionaryAsync(m => m.Id);

        decimal totalAmount = 0;
        var orderItems = new List<OrderItem>();

        foreach (var reqItem in request.Items)
        {
            if (menuItems.TryGetValue(reqItem.MenuItemId, out var dbItem))
            {
                if (dbItem.StockQuantity < reqItem.Quantity)
                {
                    return BadRequest(new { success = false, message = $"На жаль, товару '{dbItem.Name}' залишилося лише {dbItem.StockQuantity} шт." });
                }

                dbItem.StockQuantity -= reqItem.Quantity; // Списуємо товар зі складу
                totalAmount += dbItem.Price * reqItem.Quantity;
                orderItems.Add(new OrderItem
                {
                    MenuItemId = dbItem.Id,
                    Quantity = reqItem.Quantity,
                    UnitPrice = dbItem.Price // Фіксуємо ціну на момент покупки
                });
            }
            else
            {
                return BadRequest(new { success = false, message = "Один із товарів не знайдено або недоступний у вашому поточному клубі." });
            }
        }

        // 2. Створюємо замовлення
        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.Pending,
            TotalAmount = totalAmount,
            CreatedAt = DateTime.UtcNow,
            PcNumber = activeSession.Hardware.PcNumber, // Фіксуємо ПК, за яким сидить гравець
            Items = orderItems
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(); // Зберігаємо, щоб отримати OrderId

        // 3. Пробуємо списати кошти
        try
        {
            await _paymentService.PayOrderAsync(userId, order.Id, totalAmount);

            // Якщо оплата успішна, міняємо статус (припустимо, є статус Paid або InProgress)
            // order.Status = OrderStatus.Paid; // Розкоментуйте, якщо є такий статус в OrderStatus
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Оплата успішна! Замовлення прийнято." });
        }
        catch (InvalidOperationException ex)
        {
            // Якщо помилка (недостатньо коштів)
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { success = false, message = "Сталася помилка при обробці платежу." });
        }
    }
}