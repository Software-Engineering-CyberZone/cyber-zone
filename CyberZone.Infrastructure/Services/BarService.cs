using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CyberZone.Application.Common;
using CyberZone.Application.DTOs;
using CyberZone.Application.Interfaces;
using CyberZone.Domain.Entities;
using CyberZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberZone.Infrastructure.Services;

public class BarService : IBarService
{
    private readonly IApplicationDbContext _context;

    public BarService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<BarMenuItemDto>>> GetMenuItemsAsync(Guid clubId)
    {
        var items = await _context.MenuItems
            .Where(m => m.ClubId == clubId)
            .Select(m => new BarMenuItemDto
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                Category = m.Category,
                IsAvailable = m.IsAvailable,
                StockQuantity = m.StockQuantity,
                IsActive = m.IsActive,
                ImageUrl = m.ImageUrl
            })
            .ToListAsync();

        return Result.Success<IEnumerable<BarMenuItemDto>>(items);
    }

    public async Task<Result<Guid>> CreateMenuItemAsync(CreateBarMenuItemDto dto)
    {
        var clubExists = await _context.Clubs.AnyAsync(c => c.Id == dto.ClubId);
        if (!clubExists) return Result.Failure<Guid>("Club not found.");

        var menuItem = new MenuItem
        {
            ClubId = dto.ClubId,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Category = dto.Category,
            IsAvailable = dto.IsAvailable,
            StockQuantity = dto.StockQuantity,
            IsActive = dto.IsActive,
            ImageUrl = dto.ImageUrl
        };

        _context.MenuItems.Add(menuItem);
        await _context.SaveChangesAsync(default);

        return Result.Success(menuItem.Id);
    }

    public async Task<Result<bool>> UpdateMenuItemAsync(Guid id, UpdateBarMenuItemDto dto)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null) return Result.Failure<bool>("Menu item not found.");

        item.Name = dto.Name;
        item.Description = dto.Description;
        item.Price = dto.Price;
        item.Category = dto.Category;
        item.IsAvailable = dto.IsAvailable;
        item.StockQuantity = dto.StockQuantity;
        item.IsActive = dto.IsActive;
        item.ImageUrl = dto.ImageUrl;

        await _context.SaveChangesAsync(default);
        return Result.Success(true);
    }

    public async Task<Result<bool>> UpdateStockAsync(Guid id, int newQuantity)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null) return Result.Failure<bool>("Menu item not found.");
        
        if (newQuantity < 0) return Result.Failure<bool>("Quantity cannot be negative.");

        item.StockQuantity = newQuantity;
        await _context.SaveChangesAsync(default);
        return Result.Success(true);
    }

    public async Task<Result<bool>> ToggleActiveAsync(Guid id, bool isActive)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null) return Result.Failure<bool>("Menu item not found.");

        item.IsActive = isActive;
        await _context.SaveChangesAsync(default);
        return Result.Success(true);
    }

    public async Task<Result<IEnumerable<OrderDto>>> GetActiveOrdersAsync(Guid clubId)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.MenuItem)
            .Include(o => o.User)
            .Where(o => (o.Status == OrderStatus.Pending) && o.Items.Any(i => i.MenuItem.ClubId == clubId))
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                PcNumber = o.PcNumber,
                UserId = o.UserId,
                UserName = o.User.UserName,
                Items = o.Items.Select(i => new OrderItemDto
                {
                    MenuItemId = i.MenuItemId,
                    MenuItemName = i.MenuItem.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            })
            .ToListAsync();

        return Result.Success<IEnumerable<OrderDto>>(orders);
    }

    public async Task<Result<bool>> CompleteOrderAsync(Guid orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) return Result.Failure<bool>("Order not found.");

        order.Status = OrderStatus.Delivered;
        await _context.SaveChangesAsync(default);
        
        return Result.Success(true);
    }
}
