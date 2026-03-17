using CyberZone.Application.Interfaces;
using CyberZone.Domain.Entities;
using CyberZone.Infrastructure.Persistence;
using CyberZone.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CyberZone.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // EF Core – SQL Server
        services.AddDbContext<CyberZoneDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(CyberZoneDbContext).Assembly.FullName)));

        // Register DbContext as IApplicationDbContext
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<CyberZoneDbContext>());

        // ASP.NET Core Identity
        services.AddIdentity<User, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<CyberZoneDbContext>()
        .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
        });

        // Services
        services.AddScoped<PaymentService>();

        return services;
    }
}
