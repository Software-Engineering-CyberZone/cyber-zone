using CyberZone.Application.Common;
using CyberZone.Application.Interfaces;
using CyberZone.Domain.Entities;
using CyberZone.Infrastructure.ExternalApis.CheapShark;
using CyberZone.Infrastructure.Persistence;
using CyberZone.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Infrastructure.Services;
using Refit;

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

        // In-memory cache
        services.AddMemoryCache();
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));
        services.AddSingleton<ICacheService, MemoryCacheService>();

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
        services.AddScoped<IPaymentService>(sp => sp.GetRequiredService<PaymentService>());
        services.AddScoped<IClubService, ClubService>();
        services.AddScoped<IClubMapService, ClubMapService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<PaymentService>();
        services.AddScoped<CyberZone.Application.Interfaces.IUserService, CyberZone.Infrastructure.Services.UserService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IBarService, BarService>();

        // External APIs (Refit + Polly resilience)
        services.Configure<CheapSharkOptions>(configuration.GetSection(CheapSharkOptions.SectionName));
        var cheapShark = configuration.GetSection(CheapSharkOptions.SectionName).Get<CheapSharkOptions>() ?? new CheapSharkOptions();

        services.AddRefitClient<ICheapSharkApi>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(cheapShark.BaseUrl);
                c.Timeout = TimeSpan.FromSeconds(cheapShark.TimeoutSeconds);
                c.DefaultRequestHeaders.Add("User-Agent", "CyberZone/1.0");
            })
            .AddStandardResilienceHandler(o =>
            {
                o.Retry.MaxRetryAttempts = 3;
                o.Retry.Delay = TimeSpan.FromMilliseconds(300);
                o.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
                o.AttemptTimeout.Timeout = TimeSpan.FromSeconds(cheapShark.TimeoutSeconds);
                o.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(cheapShark.TimeoutSeconds * 4);
                o.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
                o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            });

        services.AddScoped<IDealsService, DealsService>();

        return services;
    }
}
