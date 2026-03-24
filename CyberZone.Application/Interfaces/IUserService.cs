using CyberZone.Application.DTOs;

namespace CyberZone.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileDto?> GetUserProfileAsync(string userId);
}