using CyberZone.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace CyberZone.Domain.Entities;

public class User : IdentityUser<Guid>, IAuditable
{
    public User()
    {
        Id = Guid.NewGuid();
    }

    public string? FullName { get; set; }
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
