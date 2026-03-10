using System.Security.Cryptography;
using CyberZone.Application.Interfaces;
using CyberZone.Domain.Enums;
using CyberZone.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CyberZone.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly CyberZoneDbContext _context;

    public IdentityService(CyberZoneDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> CreateUserAsync(string userName, string email, string password, UserRole role)
    {
        var user = new Domain.Entities.User
        {
            UserName = userName,
            Email = email,
            PasswordHash = HashPassword(password),
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user.Id;
    }

    public async Task<bool> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _context.Users
               .AsNoTracking()
           .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
            return false;

        return VerifyPassword(password, user.PasswordHash);
    }

    public async Task<Guid?> GetUserIdByEmailAsync(string email)
    {
        var user = await _context.Users
          .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);

        return user?.Id;
    }

    public async Task<bool> IsInRoleAsync(Guid userId, UserRole role)
    {
        var user = await _context.Users
            .AsNoTracking()
       .FirstOrDefaultAsync(u => u.Id == userId);

        return user?.Role == role;
    }

    /// <summary>
    /// Simple password hashing using PBKDF2/SHA-256 with a random salt.
    /// </summary>
    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 2)
            return false;

        var salt = Convert.FromBase64String(parts[0]);
        var expectedHash = Convert.FromBase64String(parts[1]);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
