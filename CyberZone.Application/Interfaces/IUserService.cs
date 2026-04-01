using CyberZone.Application.Common;
using CyberZone.Application.DTOs;

namespace CyberZone.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileDto?> GetUserProfileAsync(string userId);
    Task<Result> UpdateUserProfileAsync(EditUserProfileDto dto);
}