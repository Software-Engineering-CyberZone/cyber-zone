using CyberZone.Application.DTOs;
using CyberZone.Application.Interfaces;
using CyberZone.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CyberZone.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;

    public UserService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null) return null;

        return new UserProfileDto
        {
            Id = user.Id,
            UserName = user.UserName ?? "User",
            Email = user.Email ?? "",
            Balance = user.Balance
        };
    }
}