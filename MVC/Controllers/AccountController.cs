using CyberZone.Application.DTOs;
using CyberZone.Application.Interfaces;
using CyberZone.Domain.Entities;
using CyberZone.Infrastructure.Persistence;
using CyberZone.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Models;
using SixLabors.ImageSharp;
using System.Security.Claims;

namespace MVC.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IUserService _userService;
    private readonly PaymentService _paymentService;
    private readonly CyberZoneDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IReviewService _reviewService;
    private readonly ICacheService _cache;

    public AccountController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IUserService userService,
        PaymentService paymentService,
        CyberZoneDbContext context,
        IWebHostEnvironment environment,
        IReviewService reviewService,
        ICacheService cache)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userService = userService;
        _paymentService = paymentService;
        _context = context;
        _environment = environment;
        _reviewService = reviewService;
        _cache = cache;
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity!.IsAuthenticated) return RedirectToAction("Catalog", "Home");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName 
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction("Catalog", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity!.IsAuthenticated) return RedirectToAction("Catalog", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, isPersistent: false, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Catalog", "Home");
                }
            }

            ModelState.AddModelError(string.Empty, "пїЅпїЅпїЅпїЅпїЅпїЅпїЅ email пїЅпїЅпїЅ пїЅпїЅпїЅпїЅпїЅпїЅ.");
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ClubPartner()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ClubPartner(ClubPartnerViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "ClubRequests");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                string fileName = $"пїЅпїЅпїЅпїЅпїЅпїЅпїЅпїЅпїЅпїЅпїЅ-{DateTime.Now:yyyyMMdd-HHmmss}.txt";
                string filePath = Path.Combine(folderPath, fileName);

                string content = $"пїЅпїЅпїЅпїЅ: {DateTime.Now}\nEmail: {model.Email}\nпїЅпїЅпїЅпїЅпїЅпїЅпїЅ: {model.Phone}";

                await System.IO.File.WriteAllTextAsync(filePath, content);

                TempData["Message"] = "пїЅпїЅпїЅпїЅпїЅпїЅ пїЅпїЅпїЅпїЅпїЅпїЅ пїЅпїЅпїЅпїЅпїЅпїЅпїЅпїЅ!";
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "пїЅпїЅпїЅпїЅпїЅпїЅпїЅ пїЅпїЅпїЅ пїЅпїЅпїЅпїЅпїЅпїЅпїЅпїЅпїЅпїЅ пїЅпїЅпїЅпїЅпїЅ: " + ex.Message);
            }
        }
        return View(model);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null) return RedirectToAction("Login");

        var userProfile = await _userService.GetUserProfileAsync(userId);

        if (userProfile == null) return NotFound();

        return View(userProfile);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> MyReviews()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return RedirectToAction("Login");

        var userProfile = await _userService.GetUserProfileAsync(userId);
        if (userProfile == null) return NotFound();

        return View(userProfile.Reviews);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Balance()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null) return RedirectToAction("Login");

        var userProfile = await _userService.GetUserProfileAsync(userId);

        if (userProfile == null) return NotFound();

        // Р”РѕРґР°С”РјРѕ ViewData, С‰РѕР± Р°РЅР°Р»С–Р·Р°С‚РѕСЂ Р±Р°С‡РёРІ, С‰Рѕ РјРµС‚РѕРґРё СЂРѕР±Р»СЏС‚СЊ СЂС–Р·РЅС– СЂРµС‡С–.
        // РЈ РјР°Р№Р±СѓС‚РЅСЊРѕРјСѓ С†Рµ РјРѕР¶РЅР° РІРёРєРѕСЂРёСЃС‚Р°С‚Рё РґР»СЏ РїС–РґСЃРІС–С‡СѓРІР°РЅРЅСЏ Р°РєС‚РёРІРЅРѕРіРѕ РїСѓРЅРєС‚Сѓ РјРµРЅСЋ
        ViewData["ActiveTab"] = "Balance";

        // РЇРІРЅРѕ РІРєР°Р·СѓС”РјРѕ РЅР°Р·РІСѓ С€Р°Р±Р»РѕРЅСѓ
        return View("Balance", userProfile);
    }

    [HttpGet]
    [Authorize]
    public IActionResult TopUp()
    {
        return View(new TopUpViewModel());
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> TopUp(TopUpViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // Р¤Р•Р™РљРћР’РР™ РЁР›Р®Р—: Р’Р°Р»С–РґР°С†С–СЏ "С‚РµСЃС‚РѕРІРѕС—" РєР°СЂС‚РєРё
        // РџСЂРёР№РјР°С”РјРѕ С‚С–Р»СЊРєРё РєР°СЂС‚РєСѓ 4111111111111111 С‚Р° CVV 123
        if (model.CardNumber != "4111111111111111" || model.Cvv != "123")
        {
            ModelState.AddModelError("", "РўСЂР°РЅР·Р°РєС†С–СЋ РІС–РґС…РёР»РµРЅРѕ Р±Р°РЅРєРѕРј. Р’РёРєРѕСЂРёСЃС‚РѕРІСѓР№С‚Рµ С‚РµСЃС‚РѕРІСѓ РєР°СЂС‚РєСѓ (4111... / 123).");
            return View(model);
        }

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null || !Guid.TryParse(userIdStr, out Guid userId))
            return RedirectToAction("Login");

        try
        {
            // Р’РёРєР»РёРєР°С”РјРѕ С‚РІС–Р№ PaymentService РґР»СЏ Р·Р°РїРёСЃСѓ РІ Р‘Р”
            await _paymentService.TopUpAsync(userId, model.Amount, "РџРѕРїРѕРІРЅРµРЅРЅСЏ Р±Р°Р»Р°РЅСЃСѓ (Visa/Mastercard)");

            TempData["SuccessMessage"] = $"Р‘Р°Р»Р°РЅСЃ СѓСЃРїС–С€РЅРѕ РїРѕРїРѕРІРЅРµРЅРѕ РЅР° {model.Amount} в‚ґ!";
            return RedirectToAction("Balance");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Р’РёРЅРёРєР»Р° РїРѕРјРёР»РєР° РїСЂРё РїРѕРїРѕРІРЅРµРЅРЅС–: " + ex.Message);
            return View(model);
        }


    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> SeedMockSessions()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null || !Guid.TryParse(userIdStr, out Guid userId))
            return RedirectToAction("Login");

        // 1. РџРћР’РќР• РћР§РР©Р•РќРќРЇ: Р’РёРґР°Р»СЏС”РјРѕ РІСЃС– СЃРµСЃС–С— С‚Р° Р±СЂРѕРЅСЋРІР°РЅРЅСЏ С†СЊРѕРіРѕ РєРѕСЂРёСЃС‚СѓРІР°С‡Р°
        var oldSessions = await _context.GamingSessions.Where(s => s.UserId == userId).ToListAsync();
        if (oldSessions.Any()) _context.GamingSessions.RemoveRange(oldSessions);

        var oldBookings = await _context.Bookings.Where(b => b.UserId == userId).ToListAsync();
        if (oldBookings.Any()) _context.Bookings.RemoveRange(oldBookings);

        await _context.SaveChangesAsync();

        // 2. ІДЕАЛЬНИЙ КЛУБ: З нормальною адресою
        var user = await _userManager.FindByIdAsync(userIdStr);
        var targetClubId = user?.ManagedClubId;
        
        CyberZone.Domain.Entities.Club? club = null;
        var realAddress = new CyberZone.Domain.ValueObjects.Address("вул. Болоня, 51", "м. Київ", "Київ", "04210", "Україна");

        if (targetClubId.HasValue && targetClubId.Value != Guid.Empty)
        {
            club = await _context.Clubs.FirstOrDefaultAsync(c => c.Id == targetClubId.Value);
            if (club != null && string.IsNullOrWhiteSpace(club.Address?.Street))
            {
                club.Address = realAddress;
            }
        }
        else
        {
            club = await _context.Clubs.FirstOrDefaultAsync(c => c.Name == "Cyberclub");
            if (club == null)
            {
                club = new CyberZone.Domain.Entities.Club
                {
                    Name = "Cyberclub",
                    Address = realAddress,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Clubs.Add(club);
            }
            else if (string.IsNullOrWhiteSpace(club.Address?.Street))
            {
                club.Address = realAddress;
            }
        }

        // 3. РџР РђР’РР›Р¬РќР• Р—РђР›Р†Р—Рћ
        var hardware = await _context.Hardwares.FirstOrDefaultAsync(h => h.PcNumber == "PC-20");
        if (hardware == null)
        {
            hardware = new CyberZone.Domain.Entities.Hardware
            {
                PcNumber = "PC-20",
                Club = club!,
                Status = CyberZone.Domain.Enums.HardwareStatus.Available, // РЎС‚Р°РІРёРјРѕ Available, Р±Рѕ СЃРµСЃС–СЏ С‰Рµ РЅРµ РїРѕС‡Р°Р»Р°СЃСЏ
                CreatedAt = DateTime.UtcNow
            };
            _context.Hardwares.Add(hardware);
        }
        else
        {
            hardware.Status = CyberZone.Domain.Enums.HardwareStatus.Available; // РЎРєРёРґР°С”РјРѕ СЃС‚Р°С‚СѓСЃ
        }

        // 4. РўРђР РР¤
        var tariff = await _context.Tariffs.FirstOrDefaultAsync(t => t.Name == "Standard");
        if (tariff == null)
        {
            tariff = new CyberZone.Domain.Entities.Tariff
            {
                Name = "Standard",
                PricePerHour = 75.00m,
                CreatedAt = DateTime.UtcNow
            };
            club!.Tariffs.Add(tariff);
        }

        await _context.SaveChangesAsync();

        // 5. Р†Р”Р•РђР›Р¬РќР• Р‘Р РћРќР®Р’РђРќРќРЇ Р”Р›РЇ РўР•РЎРўРЈ (Pending)
        var booking = new CyberZone.Domain.Entities.Booking
        {
            UserId = userId,
            Hardware = hardware,
            Tariff = tariff,
            StartTime = DateTime.UtcNow, // РџРѕС‡РёРЅР°С”С‚СЊСЃСЏ РџР РЇРњРћ Р—РђР РђР—
            EndTime = DateTime.UtcNow.AddHours(2), // Р—Р°Р±СѓРєР°РЅРѕ СЂС–РІРЅРѕ РЅР° 2 РіРѕРґРёРЅРё
            Status = CyberZone.Domain.Enums.BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        _context.Bookings.Add(booking);

        // 6. Р†РЎРўРћР Р†РЇ: РљС–Р»СЊРєР° СЃС‚Р°СЂРёС… Р·Р°РІРµСЂС€РµРЅРёС… СЃРµСЃС–Р№ РґР»СЏ РєСЂР°СЃРё
        var completedSessions = new List<CyberZone.Domain.Entities.GamingSession>
        {
            new CyberZone.Domain.Entities.GamingSession
            {
                UserId = userId,
                Hardware = hardware,
                Tariff = tariff,
                StartTime = DateTime.UtcNow.AddDays(-1).AddHours(-2),
                EndTime = DateTime.UtcNow.AddDays(-1),
                Status = CyberZone.Domain.Enums.SessionStatus.Completed,
                TotalCost = 150.00m,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new CyberZone.Domain.Entities.GamingSession
            {
                UserId = userId,
                Hardware = hardware,
                Tariff = tariff,
                StartTime = DateTime.UtcNow.AddDays(-2).AddHours(-5),
                EndTime = DateTime.UtcNow.AddDays(-2).AddHours(-2),
                Status = CyberZone.Domain.Enums.SessionStatus.Completed,
                TotalCost = 225.00m,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };
        _context.GamingSessions.AddRange(completedSessions);
#pragma warning disable S1075
        // 7. РЎР†Р”Р•РњРћ РўРћР’РђР Р Р”Р›РЇ Р‘РђР РЈ (MenuItems)
        if (!await _context.MenuItems.AnyAsync())
        {
            var menuItems = new List<CyberZone.Domain.Entities.MenuItem>
            {
                new CyberZone.Domain.Entities.MenuItem { Name = "CocaCola", Description = "330 РјР»", Price = 50.00m, Category = "Drinks", ImageUrl = "/images/cocacola.png", IsAvailable = true, Club = club!, CreatedAt = DateTime.UtcNow },
                new CyberZone.Domain.Entities.MenuItem { Name = "Fanta", Description = "330 РјР»", Price = 50.00m, Category = "Drinks", ImageUrl = "/images/fanta.png", IsAvailable = true, Club = club!, CreatedAt = DateTime.UtcNow },
                new CyberZone.Domain.Entities.MenuItem { Name = "Sprite", Description = "330 РјР»", Price = 50.00m, Category = "Drinks", ImageUrl = "/images/sprite.png", IsAvailable = true, Club = club!, CreatedAt = DateTime.UtcNow },
                new CyberZone.Domain.Entities.MenuItem { Name = "LayвЂ™s", Description = "120 РіСЂ", Price = 70.00m, Category = "Snacks", ImageUrl = "/images/lays.png", IsAvailable = true, Club = club!, CreatedAt = DateTime.UtcNow },
                new CyberZone.Domain.Entities.MenuItem { Name = "Doritos", Description = "100 РіСЂ", Price = 80.00m, Category = "Snacks", ImageUrl = "/images/doritos.png", IsAvailable = true, Club = club!, CreatedAt = DateTime.UtcNow }
            };
            _context.MenuItems.AddRange(menuItems);
        }
#pragma warning restore S1075
        await _context.SaveChangesAsync();

        return RedirectToAction("Sessions");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> EditProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return RedirectToAction("Login");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var model = new EditProfileViewModel
        {
            Email = user.Email ?? "",
            FullName = user.FullName,
            Bio = user.Bio,
            Phone = user.Phone,
            Location = user.Location,
            WebsiteUrl = user.WebsiteUrl,
            ExistingProfileImagePath = user.ProfileImagePath
        };

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return RedirectToAction("Login");

        if (!ModelState.IsValid)
        {
            model.ExistingProfileImagePath = (await _userManager.FindByIdAsync(userId))?.ProfileImagePath;
            return View(model);
        }

        // HTML-СЃР°РЅС–С‚РёР·Р°С†С–СЏ Р±С–РѕРіСЂР°С„С–С—
        string? sanitizedBio = model.Bio != null
            ? System.Net.WebUtility.HtmlEncode(model.Bio)
            : null;

        // РћР±СЂРѕР±РєР° Р·Р°РІР°РЅС‚Р°Р¶РµРЅРЅСЏ Р·РѕР±СЂР°Р¶РµРЅРЅСЏ
        string? newImagePath = null;
        if (model.ProfileImage != null && model.ProfileImage.Length > 0)
        {
            var validationError = ValidateImage(model.ProfileImage);
            if (validationError != null)
            {
                ModelState.AddModelError("ProfileImage", validationError);
                model.ExistingProfileImagePath = (await _userManager.FindByIdAsync(userId))?.ProfileImagePath;
                return View(model);
            }

            newImagePath = await SaveProfileImageAsync(model.ProfileImage, userId);
        }

        var existingUser = await _userManager.FindByIdAsync(userId);
        var oldImagePath = existingUser?.ProfileImagePath;

        var dto = new EditUserProfileDto
        {
            UserId = userId,
            Email = model.Email,
            FullName = model.FullName,
            Bio = sanitizedBio,
            Phone = model.Phone,
            Location = model.Location,
            WebsiteUrl = model.WebsiteUrl,
            ProfileImagePath = newImagePath ?? oldImagePath
        };

        var result = await _userService.UpdateUserProfileAsync(dto);

        if (result.IsFailure)
        {
            // РЇРєС‰Рѕ РѕРЅРѕРІР»РµРЅРЅСЏ Р‘Р” РЅРµ РІРґР°Р»РѕСЃСЊ, РІРёРґР°Р»СЏС”РјРѕ С‰РѕР№РЅРѕ Р·Р±РµСЂРµР¶РµРЅРёР№ С„Р°Р№Р»
            if (newImagePath != null)
                DeleteProfileImage(newImagePath);

            ModelState.AddModelError("", result.Error!);
            model.ExistingProfileImagePath = oldImagePath;
            return View(model);
        }

        // РЇРєС‰Рѕ РѕРЅРѕРІР»РµРЅРЅСЏ СѓСЃРїС–С€РЅРµ С– С” РЅРѕРІРµ С„РѕС‚Рѕ вЂ” РІРёРґР°Р»СЏС”РјРѕ СЃС‚Р°СЂРµ
        if (newImagePath != null && oldImagePath != null)
            DeleteProfileImage(oldImagePath);

        TempData["SuccessMessage"] = "РџСЂРѕС„С–Р»СЊ СѓСЃРїС–С€РЅРѕ РѕРЅРѕРІР»РµРЅРѕ!";
        return RedirectToAction("Profile");
    }

    private static string? ValidateImage(IFormFile file)
    {
        // РњР°РєСЃРёРјСѓРј 5 РњР‘
        if (file.Length > 5 * 1024 * 1024)
            return "Р РѕР·РјС–СЂ С„Р°Р№Р»Сѓ РЅРµ РјРѕР¶Рµ РїРµСЂРµРІРёС‰СѓРІР°С‚Рё 5 РњР‘.";

        // РџРµСЂРµРІС–СЂРєР° MIME-С‚РёРїСѓ
        var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif" };
        if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            return "Р”РѕР·РІРѕР»РµРЅС– С„РѕСЂРјР°С‚Рё: JPG, PNG, GIF.";

        // РџРµСЂРµРІС–СЂРєР° СЂРѕР·С€РёСЂРµРЅРЅСЏ (Р·Р°С…РёСЃС‚ РІС–Рґ РїС–РґРјС–РЅРё MIME)
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return "Р”РѕР·РІРѕР»РµРЅС– С„РѕСЂРјР°С‚Рё: JPG, PNG, GIF.";

        // РџРµСЂРµРІС–СЂРєР° СЂРѕР·РјС–СЂС–РІ Р·РѕР±СЂР°Р¶РµРЅРЅСЏ
        using var stream = file.OpenReadStream();
        using var image = Image.Load(stream);
        if (image.Width < 300 || image.Height < 300)
            return "РњС–РЅС–РјР°Р»СЊРЅРёР№ СЂРѕР·РјС–СЂ Р·РѕР±СЂР°Р¶РµРЅРЅСЏ: 300x300 РїС–РєСЃРµР»С–РІ.";
        if (image.Width > 2000 || image.Height > 2000)
            return "РњР°РєСЃРёРјР°Р»СЊРЅРёР№ СЂРѕР·РјС–СЂ Р·РѕР±СЂР°Р¶РµРЅРЅСЏ: 2000x2000 РїС–РєСЃРµР»С–РІ.";

        return null;
    }

    private async Task<string> SaveProfileImageAsync(IFormFile file, string userId)
    {
        var uploadsDir = Path.Combine(_environment.WebRootPath, "images", "profiles");
        if (!Directory.Exists(uploadsDir))
            Directory.CreateDirectory(uploadsDir);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{userId}_{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/images/profiles/{fileName}";
    }

    private void DeleteProfileImage(string relativePath)
    {
        var fullPath = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/'));
        if (System.IO.File.Exists(fullPath))
            System.IO.File.Delete(fullPath);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Sessions()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null || !Guid.TryParse(userIdStr, out Guid userId))
            return RedirectToAction("Login");

        var expiredSessions = await _context.GamingSessions
            .Include(s => s.Tariff)
            .Include(s => s.Hardware)
            .Where(s => s.UserId == userId
                     && s.Status == CyberZone.Domain.Enums.SessionStatus.Active
                     && s.EndTime.HasValue
                     && s.EndTime.Value <= DateTime.UtcNow) // РЇРєС‰Рѕ С‡Р°СЃ Р·Р°РІРµСЂС€РµРЅРЅСЏ РІР¶Рµ РЅР°СЃС‚Р°РІ Р°Р±Рѕ РјРёРЅСѓРІ
            .ToListAsync();

        if (expiredSessions.Any())
        {
            var affectedClubs = new HashSet<Guid>();
            foreach (var session in expiredSessions)
            {
                session.EndSession(); // Р’РёРєР»РёРєР°С”РјРѕ РґРѕРјРµРЅРЅРёР№ РјРµС‚РѕРґ (СЂР°С…СѓС” РіСЂРѕС€С–, РјС–РЅСЏС” СЃС‚Р°С‚СѓСЃ)

                // Р—РІС–Р»СЊРЅСЏС”РјРѕ РџРљ
                if (session.Hardware != null)
                {
                    session.Hardware.Status = CyberZone.Domain.Enums.HardwareStatus.Available;
                    affectedClubs.Add(session.Hardware.ClubId);
                }

                // Закриваємо пов'язаний Booking
                var relatedBooking = await _context.Bookings
                    .Where(b => b.HardwareId == session.HardwareId
                             && b.UserId == session.UserId
                             && b.Status == CyberZone.Domain.Enums.BookingStatus.Active)
                    .OrderByDescending(b => b.StartTime)
                    .FirstOrDefaultAsync();
                if (relatedBooking != null)
                    relatedBooking.Status = CyberZone.Domain.Enums.BookingStatus.Completed;
            }
<<<<<<< Updated upstream
            await _context.SaveChangesAsync(); // Зберігаємо зміни в базу

            foreach (var clubId in affectedClubs)
                _cache.Remove(CyberZone.Application.Interfaces.CacheKeys.ClubMap(clubId));
=======
            await _context.SaveChangesAsync(); // Р—Р±РµСЂС–РіР°С”РјРѕ Р·РјС–РЅРё РІ Р±Р°Р·Сѓ
>>>>>>> Stashed changes
        }

        var viewModels = new List<SessionItemViewModel>();

        // РћС‚СЂРёРјСѓС”РјРѕ С‡Р°СЃРѕРІРёР№ РїРѕСЏСЃ РЈРєСЂР°С—РЅРё (Р±РµР·РїРµС‡РЅРёР№ СЃРїРѕСЃС–Р± РґР»СЏ Windows С‚Р° Linux/Mac)
        TimeZoneInfo kyivZone;
        try { kyivZone = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time"); } // Р”Р»СЏ Windows
        catch { kyivZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv"); } // Р”Р»СЏ Linux/macOS

        // 1. РўСЏРіРЅРµРјРѕ РђРљРўРР’РќР† С‚Р° Р—РђР’Р•Р РЁР•РќР† СЃРµСЃС–С—
        var sessions = await _context.GamingSessions
            .Include(s => s.Hardware).ThenInclude(h => h.Club)
            .Include(s => s.Tariff)
            .Where(s => s.UserId == userId)
            .ToListAsync();

        // РћС‚СЂРёРјСѓС”РјРѕ ClubId-Рё, РґР»СЏ СЏРєРёС… С” РІС–РґРіСѓРєРё
        var reviewedClubIds = await _context.Reviews
            .Where(r => r.UserId == userId)
            .Select(r => r.ClubId)
            .ToListAsync();

        viewModels.AddRange(sessions.Select(s =>
        {
            // РџРµСЂРµРІРѕРґРёРјРѕ UTC С‡Р°СЃ Сѓ РљРёС—РІСЃСЊРєРёР№ РґР»СЏ РєСЂР°СЃРёРІРѕРіРѕ РІС–РґРѕР±СЂР°Р¶РµРЅРЅСЏ
            var localStartTime = TimeZoneInfo.ConvertTimeFromUtc(s.StartTime, kyivZone);
            var clubId = s.Hardware?.Club?.Id ?? Guid.Empty;

            return new SessionItemViewModel
            {
                Id = s.Id,
                ClubName = s.Hardware?.Club?.Name ?? "CyberZone Club",
                Address = s.Hardware?.Club?.Address?.ToString() ?? "РђРґСЂРµСЃР° РЅРµ РІРєР°Р·Р°РЅР°",
                PcNumber = s.Hardware?.PcNumber ?? "N/A",

                // Р’РёРєРѕСЂРёСЃС‚РѕРІСѓС”РјРѕ РЅР°С€ Р»РѕРєР°Р»СЊРЅРёР№ С‡Р°СЃ РґР»СЏ С‚РµРєСЃС‚Сѓ
                Date = localStartTime.ToString("dd.MM.yyyy"),
                Time = localStartTime.ToString("HH:mm"),

                Duration = s.EndTime.HasValue
                    ? Math.Round((s.EndTime.Value - s.StartTime).TotalHours, 1).ToString() + " РіРѕРґ."
                    : "РўСЂРёРІР°С”",
                SessionState = s.Status == CyberZone.Domain.Enums.SessionStatus.Active ? "Active" : "Completed",
                ClubId = clubId,
                HasReview = reviewedClubIds.Contains(clubId),

                // Р”Р»СЏ JS С– СЃРѕСЂС‚СѓРІР°РЅРЅСЏ Р·Р°Р»РёС€Р°С”РјРѕ РѕСЂРёРіС–РЅР°Р»СЊРЅРёР№ UTC (Р±СЂР°СѓР·РµСЂ СЃР°Рј Р№РѕРіРѕ Р·СЂРѕР·СѓРјС–С”)
                SortDate = s.StartTime,
                TargetTime = s.Status == CyberZone.Domain.Enums.SessionStatus.Active && s.EndTime.HasValue
                    ? s.EndTime.Value.ToString("yyyy-MM-ddTHH:mm:ssZ")
                    : ""
            };
        }));

        // 2. РўСЏРіРЅРµРјРѕ РџР›РђРќРћР’РђРќР† СЃРµСЃС–С—
        var bookings = await _context.Bookings
            .Include(b => b.Hardware).ThenInclude(h => h.Club)
            .Include(b => b.Tariff)
            .Where(b => b.UserId == userId && b.Status == CyberZone.Domain.Enums.BookingStatus.Pending)
            .ToListAsync();

        viewModels.AddRange(bookings.Select(b =>
        {
            // РџРµСЂРµРІРѕРґРёРјРѕ UTC С‡Р°СЃ Сѓ РљРёС—РІСЃСЊРєРёР№ РґР»СЏ РєСЂР°СЃРёРІРѕРіРѕ РІС–РґРѕР±СЂР°Р¶РµРЅРЅСЏ
            var localStartTime = TimeZoneInfo.ConvertTimeFromUtc(b.StartTime, kyivZone);

            return new SessionItemViewModel
            {
                Id = b.Id,
                ClubName = b.Hardware?.Club?.Name ?? "CyberZone Club",
                Address = b.Hardware?.Club?.Address?.ToString() ?? "РђРґСЂРµСЃР° РЅРµ РІРєР°Р·Р°РЅР°",
                PcNumber = b.Hardware?.PcNumber ?? "N/A",

                // Р’РёРєРѕСЂРёСЃС‚РѕРІСѓС”РјРѕ РЅР°С€ Р»РѕРєР°Р»СЊРЅРёР№ С‡Р°СЃ РґР»СЏ С‚РµРєСЃС‚Сѓ
                Date = localStartTime.ToString("dd.MM.yyyy"),
                Time = localStartTime.ToString("HH:mm"),

                Duration = Math.Round((b.EndTime - b.StartTime).TotalHours, 1).ToString() + " РіРѕРґ.",
                SessionState = "Pending",
                ClubId = b.Hardware?.Club?.Id ?? Guid.Empty,
                SortDate = b.StartTime
            };
        }));

        viewModels = viewModels.OrderByDescending(v => v.SortDate).ToList();

        return View(viewModels);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> StartSession(Guid id)
    {
        var booking = await _context.Bookings
            .Include(b => b.Hardware)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking != null && booking.Status == CyberZone.Domain.Enums.BookingStatus.Pending)
        {
            booking.Status = CyberZone.Domain.Enums.BookingStatus.Confirmed;
            var newSession = booking.TransitionToSession();

            var plannedDuration = booking.EndTime - booking.StartTime;
            newSession.EndTime = newSession.StartTime.Add(plannedDuration);

            Guid? clubId = null;
            if (booking.Hardware != null)
            {
                booking.Hardware.Status = CyberZone.Domain.Enums.HardwareStatus.Busy;
                clubId = booking.Hardware.ClubId;
            }

            _context.GamingSessions.Add(newSession);
            await _context.SaveChangesAsync();

            if (clubId.HasValue)
                _cache.Remove(CyberZone.Application.Interfaces.CacheKeys.ClubMap(clubId.Value));
        }

        return RedirectToAction("Sessions");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CancelSession(Guid id)
    {
        // Р”Р»СЏ Р±СЂРѕРЅСЋРІР°РЅСЊ РєРЅРѕРїРєР° "РЎРєР°СЃСѓРІР°С‚Рё" РїСЂРѕСЃС‚Рѕ РјС–РЅСЏС” СЃС‚Р°С‚СѓСЃ
        var booking = await _context.Bookings.FindAsync(id);

        if (booking != null && booking.Status == CyberZone.Domain.Enums.BookingStatus.Pending)
        {
            booking.Status = CyberZone.Domain.Enums.BookingStatus.Cancelled;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Sessions");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> EndSession(Guid id)
    {
        // Рђ С†Рµ РІР¶Рµ РґР»СЏ РђРљРўРР’РќРћР‡ СЃРµСЃС–С— (РІРµР»РёРєР° РєР°СЂС‚РєР°)
        var session = await _context.GamingSessions
            .Include(s => s.Tariff)
            .Include(s => s.Hardware)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session != null && session.Status == CyberZone.Domain.Enums.SessionStatus.Active)
        {
            session.EndSession(); // Р’РёРєР»РёРєР°С”РјРѕ С‚РІС–Р№ РґРѕРјРµРЅРЅРёР№ РјРµС‚РѕРґ СЂРѕР·СЂР°С…СѓРЅРєСѓ

<<<<<<< Updated upstream
            Guid? clubId = null;
            // Звільняємо ПК
=======
            // Р—РІС–Р»СЊРЅСЏС”РјРѕ РџРљ
>>>>>>> Stashed changes
            if (session.Hardware != null)
            {
                session.Hardware.Status = CyberZone.Domain.Enums.HardwareStatus.Available;
                clubId = session.Hardware.ClubId;
            }

            // Закриваємо пов'язаний Booking (щоб не блокував overlap при новому бронюванні)
            var relatedBooking = await _context.Bookings
                .Where(b => b.HardwareId == session.HardwareId
                         && b.UserId == session.UserId
                         && b.Status == CyberZone.Domain.Enums.BookingStatus.Active)
                .OrderByDescending(b => b.StartTime)
                .FirstOrDefaultAsync();
            if (relatedBooking != null)
                relatedBooking.Status = CyberZone.Domain.Enums.BookingStatus.Completed;

            await _context.SaveChangesAsync();

            if (clubId.HasValue)
                _cache.Remove(CyberZone.Application.Interfaces.CacheKeys.ClubMap(clubId.Value));
        }

        return RedirectToAction("Sessions");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> LeaveReview(CreateReviewDto dto)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null || !Guid.TryParse(userIdStr, out Guid userId))
            return RedirectToAction("Login");

        var result = await _reviewService.AddReviewAsync(userId, dto);

        if (result.IsFailure)
            TempData["Error"] = result.Error;
        else
            TempData["SuccessMessage"] = "Р’С–РґРіСѓРє СѓСЃРїС–С€РЅРѕ Р·Р°Р»РёС€РµРЅРѕ!";

        return RedirectToAction("Sessions");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> EditReview(Guid id, int rating, string comment)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdStr, out Guid userId))
        {
            await _reviewService.UpdateReviewAsync(id, userId, rating, comment);
        }

        return RedirectToAction("MyReviews");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> DeleteReview(Guid id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdStr, out Guid userId))
        {
            await _reviewService.DeleteReviewAsync(id, userId);
        }

        return RedirectToAction("MyReviews");
    }
}