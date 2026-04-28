using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CyberZone.Application.Common;
using CyberZone.Application.DTOs;

namespace CyberZone.Application.Interfaces;

public interface IBarService
{
    Task<Result<BarMenuItemDto>> GetMenuItemAsync(Guid id);
    Task<Result<IEnumerable<BarMenuItemDto>>> GetMenuItemsAsync(Guid clubId);
    Task<Result<Guid>> CreateMenuItemAsync(CreateBarMenuItemDto dto);
    Task<Result<bool>> UpdateMenuItemAsync(Guid id, UpdateBarMenuItemDto dto);
    Task<Result<bool>> UpdateStockAsync(Guid id, int newQuantity);
    Task<Result<bool>> ToggleActiveAsync(Guid id, bool isActive);
    Task<Result<IEnumerable<OrderDto>>> GetActiveOrdersAsync(Guid clubId);
    Task<Result<bool>> CompleteOrderAsync(Guid orderId);
}
