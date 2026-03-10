using CyberZone.Domain.Common;
using CyberZone.Domain.Enums;

namespace CyberZone.Domain.Entities;

public class User : EntityBase, IAuditable
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public UserRole Role { get; set; } = UserRole.Client;
    public decimal Balance { get; set; }

    /// <summary>
    /// JSONB column storing linked gaming accounts (e.g., Steam, Epic, Battle.net).
    /// </summary>
    public Dictionary<string, string> LinkedAccounts { get; set; } = [];

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public ICollection<Booking> Bookings { get; set; } = [];
    public ICollection<GamingSession> GamingSessions { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<Transaction> Transactions { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
}
