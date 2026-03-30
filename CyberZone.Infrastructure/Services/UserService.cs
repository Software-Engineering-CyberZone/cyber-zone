using CyberZone.Application.DTOs;
using CyberZone.Application.Interfaces;
using CyberZone.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CyberZone.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly IApplicationDbContext _context;

    public UserService(UserManager<User> userManager, IApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null) return null;

        var parsedUserId = Guid.Parse(userId);

        var transactions = await _context.Transactions
            .Where(t => t.UserId == parsedUserId)
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                Type = t.Type,
                Amount = t.Amount,
                Description = t.Description,
                TransactionDate = t.TransactionDate,
                UserId = t.UserId,
                ReferenceId = t.ReferenceId
            })
            .ToListAsync();

        return new UserProfileDto
        {
            Id = user.Id,
            UserName = user.UserName ?? "User",
            Email = user.Email ?? "",
            Balance = user.Balance,
            Transactions = transactions
        };
    }



}