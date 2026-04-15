using CyberZone.Application.Common;
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

        var reviews = await _context.Reviews
            .Where(r => r.UserId == parsedUserId)
            .Include(r => r.Club)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new UserReviewDto
            {
                Id = r.Id,
                ClubId = r.ClubId,
                ClubName = r.Club.Name,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new UserProfileDto
        {
            Id = user.Id,
            UserName = user.UserName ?? "User",
            Email = user.Email ?? "",
            FullName = user.FullName,
            Balance = user.Balance,
            Bio = user.Bio,
            Phone = user.Phone,
            Location = user.Location,
            WebsiteUrl = user.WebsiteUrl,
            ProfileImagePath = user.ProfileImagePath,
            Transactions = transactions,
            Reviews = reviews
        };
    }

    public async Task<Result> UpdateUserProfileAsync(EditUserProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
            return Result.Failure("Користувача не знайдено.");

        user.Email = dto.Email;
        user.UserName = dto.Email;
        user.FullName = dto.FullName;
        user.Bio = dto.Bio;
        user.Phone = dto.Phone;
        user.Location = dto.Location;
        user.WebsiteUrl = dto.WebsiteUrl;
        user.ProfileImagePath = dto.ProfileImagePath;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return Result.Failure(errors);
        }

        return Result.Success();
    }
}