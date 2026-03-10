using CyberZone.Application.Interfaces;
using CyberZone.Infrastructure.Persistence;
using CyberZone.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CyberZone.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // EF Core — SQL Server
        services.AddDbContext<CyberZoneDbContext>(options =>
    options.UseSqlServer(
         configuration.GetConnectionString("DefaultConnection"),
           b => b.MigrationsAssembly(typeof(CyberZoneDbContext).Assembly.FullName)));

        // Register DbContext as IApplicationDbContext
        services.AddScoped<IApplicationDbContext>(provider =>
         provider.GetRequiredService<CyberZoneDbContext>());

        // Services
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<PaymentService>();

        return services;
    }
}
