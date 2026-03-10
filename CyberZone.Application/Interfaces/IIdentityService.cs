using CyberZone.Domain.Enums;

namespace CyberZone.Application.Interfaces;

public interface IIdentityService
{
    Task<Guid> CreateUserAsync(string userName, string email, string password, UserRole role);
    Task<bool> ValidateCredentialsAsync(string email, string password);
    Task<Guid?> GetUserIdByEmailAsync(string email);
    Task<bool> IsInRoleAsync(Guid userId, UserRole role);
}
