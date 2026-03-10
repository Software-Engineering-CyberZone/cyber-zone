using CyberZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CyberZone.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Club> Clubs { get; }
    DbSet<Hardware> Hardwares { get; }
    DbSet<Tariff> Tariffs { get; }
    DbSet<Booking> Bookings { get; }
    DbSet<GamingSession> GamingSessions { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<MenuItem> MenuItems { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Review> Reviews { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
