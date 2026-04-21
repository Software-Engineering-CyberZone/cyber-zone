using CyberZone.Domain.Common;
using CyberZone.Domain.Enums;
using CyberZone.Domain.ValueObjects;

namespace CyberZone.Domain.Entities;

public class Club : EntityBase, IAuditable
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Address Address { get; set; } = new();
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public double Rating { get; set; }
    /// <summary>
    /// JSONB column storing work hours (e.g., {"Monday": "09:00-23:00"}).
    /// </summary>
    public Dictionary<string, string> WorkHours { get; set; } = [];

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public ICollection<Hardware> Hardwares { get; set; } = [];
    public ICollection<Tariff> Tariffs { get; set; } = new List<Tariff>();
    public ICollection<MenuItem> MenuItems { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<User> StaffMembers { get; set; } = [];
}
