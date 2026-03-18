using CyberZone.Domain.Entities;
using CyberZone.Domain.Enums;
using CyberZone.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Seed roles
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CyberZone.Infrastructure.Persistence.CyberZoneDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    foreach (var role in Enum.GetNames<UserRole>())
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role });
        }
    }
    if (!dbContext.Clubs.Any())
    {
        dbContext.Clubs.Add(new Club
        {
            Name = "CyberPro Arena",
            Rating = 5.0,
            Email = "info@cyberpro.ua",
            Phone = "+380991234567",
            Address = new CyberZone.Domain.ValueObjects.Address
            {
                City = "Київ",
                Street = "вул. Болоня",
                State = "Київська обл.",
                ZipCode = "01000",
                Country = "Україна"
            }
        });

        await dbContext.SaveChangesAsync();
    }
}

app.UseSerilogRequestLogging();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
