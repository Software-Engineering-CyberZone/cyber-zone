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
    public async Task<IActionResult> Bar() // <-- ПРИБРАЛИ sessionId
    {
        // 1. Дістаємо всі ДОСТУПНІ товари з бази
        var items = await _context.MenuItems
            .Where(m => m.IsAvailable)
            .ToListAsync();

        // 2. Групуємо їх за категоріями
        var model = new BarViewModel
        {
            Drinks = items.Where(i => i.Category == "Drinks").ToList(),
            Snacks = items.Where(i => i.Category == "Snacks").ToList()
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

        // 1. Дістаємо актуальні товари з бази для формування цін
        var itemIds = request.Items.Select(i => i.MenuItemId).ToList();
        var menuItems = await _context.MenuItems
            .Where(m => itemIds.Contains(m.Id) && m.IsAvailable)
            .ToDictionaryAsync(m => m.Id);

        decimal totalAmount = 0;
        var orderItems = new List<OrderItem>();

        foreach (var reqItem in request.Items)
        {
            if (menuItems.TryGetValue(reqItem.MenuItemId, out var dbItem))
            {
                totalAmount += dbItem.Price * reqItem.Quantity;
                orderItems.Add(new OrderItem
                {
                    MenuItemId = dbItem.Id,
                    Quantity = reqItem.Quantity,
                    UnitPrice = dbItem.Price // Фіксуємо ціну на момент покупки
                });
            }
        }

        // 2. Створюємо замовлення
        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.Pending,
            TotalAmount = totalAmount,
            CreatedAt = DateTime.UtcNow,
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