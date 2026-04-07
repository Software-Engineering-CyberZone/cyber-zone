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

    public AccountController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IUserService userService,
        PaymentService paymentService,
        CyberZoneDbContext context,
        IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userService = userService;
        _paymentService = paymentService;
        _context = context;
        _environment = environment;
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

            ModelState.AddModelError(string.Empty, "������� email ��� ������.");
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

                string fileName = $"�����������-{DateTime.Now:yyyyMMdd-HHmmss}.txt";
                string filePath = Path.Combine(folderPath, fileName);

                string content = $"����: {DateTime.Now}\nEmail: {model.Email}\n�������: {model.Phone}";

                await System.IO.File.WriteAllTextAsync(filePath, content);

                TempData["Message"] = "������ ������ ��������!";
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "������� ��� ���������� �����: " + ex.Message);
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
    public async Task<IActionResult> Balance()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null) return RedirectToAction("Login");

        var userProfile = await _userService.GetUserProfileAsync(userId);

        if (userProfile == null) return NotFound();

        // Додаємо ViewData, щоб аналізатор бачив, що методи роблять різні речі.
        // У майбутньому це можна використати для підсвічування активного пункту меню
        ViewData["ActiveTab"] = "Balance";

        // Явно вказуємо назву шаблону
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

        // ФЕЙКОВИЙ ШЛЮЗ: Валідація "тестової" картки
        // Приймаємо тільки картку 4111111111111111 та CVV 123
        if (model.CardNumber != "4111111111111111" || model.Cvv != "123")
        {
            ModelState.AddModelError("", "Транзакцію відхилено банком. Використовуйте тестову картку (4111... / 123).");
            return View(model);
        }

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null || !Guid.TryParse(userIdStr, out Guid userId))
            return RedirectToAction("Login");

        try
        {
            // Викликаємо твій PaymentService для запису в БД
            await _paymentService.TopUpAsync(userId, model.Amount, "Поповнення балансу (Visa/Mastercard)");

            TempData["SuccessMessage"] = $"Баланс успішно поповнено на {model.Amount} ₴!";
            return RedirectToAction("Balance");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Виникла помилка при поповненні: " + ex.Message);
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

        // 1. ПОВНЕ ОЧИЩЕННЯ: Видаляємо всі сесії та бронювання цього користувача
        var oldSessions = await _context.GamingSessions.Where(s => s.UserId == userId).ToListAsync();
        if (oldSessions.Any()) _context.GamingSessions.RemoveRange(oldSessions);

        var oldBookings = await _context.Bookings.Where(b => b.UserId == userId).ToListAsync();
        if (oldBookings.Any()) _context.Bookings.RemoveRange(oldBookings);

        await _context.SaveChangesAsync();

        // 2. ІДЕАЛЬНИЙ КЛУБ: З нормальною адресою
        var club = await _context.Clubs.FirstOrDefaultAsync(c => c.Name == "Cyber Pro Arena");

        var realAddress = new CyberZone.Domain.ValueObjects.Address("вул. Болоня, 51", "м. Київ", "Київ", "04210", "Україна");

        if (club == null)
        {
            club = new CyberZone.Domain.Entities.Club
            {
                Name = "Cyber Pro Arena",
                Address = realAddress, // Додаємо справжню адресу!
                CreatedAt = DateTime.UtcNow
            };
            _context.Clubs.Add(club);
        }
        else if (string.IsNullOrWhiteSpace(club.Address.Street))
        {
            // Якщо клуб вже був створений раніше з кривою адресою — ми це виправимо
            club.Address = realAddress;
        }

        // 3. ПРАВИЛЬНЕ ЗАЛІЗО
        var hardware = await _context.Hardwares.FirstOrDefaultAsync(h => h.PcNumber == "PC-20");
        if (hardware == null)
        {
            hardware = new CyberZone.Domain.Entities.Hardware
            {
                PcNumber = "PC-20",
                Club = club,
                Status = CyberZone.Domain.Enums.HardwareStatus.Available, // Ставимо Available, бо сесія ще не почалася
                CreatedAt = DateTime.UtcNow
            };
            _context.Hardwares.Add(hardware);
        }
        else
        {
            hardware.Status = CyberZone.Domain.Enums.HardwareStatus.Available; // Скидаємо статус
        }

        // 4. ТАРИФ
        var tariff = await _context.Tariffs.FirstOrDefaultAsync(t => t.Name == "Standard");
        if (tariff == null)
        {
            tariff = new CyberZone.Domain.Entities.Tariff
            {
                Name = "Standard",
                PricePerHour = 75.00m,
                CreatedAt = DateTime.UtcNow
            };
            club.Tariffs.Add(tariff);
        }

        await _context.SaveChangesAsync();

        // 5. ІДЕАЛЬНЕ БРОНЮВАННЯ ДЛЯ ТЕСТУ (Pending)
        var booking = new CyberZone.Domain.Entities.Booking
        {
            UserId = userId,
            Hardware = hardware,
            Tariff = tariff,
            StartTime = DateTime.UtcNow, // Починається ПРЯМО ЗАРАЗ
            EndTime = DateTime.UtcNow.AddHours(2), // Забукано рівно на 2 години
            Status = CyberZone.Domain.Enums.BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        _context.Bookings.Add(booking);

        // 6. ІСТОРІЯ: Кілька старих завершених сесій для краси
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
        // 7. СІДЕМО ТОВАРИ ДЛЯ БАРУ (MenuItems)
        if (!await _context.MenuItems.AnyAsync())
        {
            var menuItems = new List<CyberZone.Domain.Entities.MenuItem>
            {
                new CyberZone.Domain.Entities.MenuItem { Name = "CocaCola", Description = "330 мл", Price = 50.00m, Category = "Drinks", ImageUrl = "/images/cocacola.png", IsAvailable = true, Club = club, CreatedAt = DateTime.UtcNow },
                new CyberZone.Domain.Entities.MenuItem { Name = "Fanta", Description = "330 мл", Price = 50.00m, Category = "Drinks", ImageUrl = "/images/fanta.png", IsAvailable = true, Club = club, CreatedAt = DateTime.UtcNow },
                new CyberZone.Domain.Entities.MenuItem { Name = "Sprite", Description = "330 мл", Price = 50.00m, Category = "Drinks", ImageUrl = "/images/sprite.png", IsAvailable = true, Club = club, CreatedAt = DateTime.UtcNow },
                new CyberZone.Domain.Entities.MenuItem { Name = "Lay’s", Description = "120 гр", Price = 70.00m, Category = "Snacks", ImageUrl = "/images/lays.png", IsAvailable = true, Club = club, CreatedAt = DateTime.UtcNow },
                new CyberZone.Domain.Entities.MenuItem { Name = "Doritos", Description = "100 гр", Price = 80.00m, Category = "Snacks", ImageUrl = "/images/doritos.png", IsAvailable = true, Club = club, CreatedAt = DateTime.UtcNow }
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

        // HTML-санітизація біографії
        string? sanitizedBio = model.Bio != null
            ? System.Net.WebUtility.HtmlEncode(model.Bio)
            : null;

        // Обробка завантаження зображення
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
            // Якщо оновлення БД не вдалось, видаляємо щойно збережений файл
            if (newImagePath != null)
                DeleteProfileImage(newImagePath);

            ModelState.AddModelError("", result.Error!);
            model.ExistingProfileImagePath = oldImagePath;
            return View(model);
        }

        // Якщо оновлення успішне і є нове фото — видаляємо старе
        if (newImagePath != null && oldImagePath != null)
            DeleteProfileImage(oldImagePath);

        TempData["SuccessMessage"] = "Профіль успішно оновлено!";
        return RedirectToAction("Profile");
    }

    private static string? ValidateImage(IFormFile file)
    {
        // Максимум 5 МБ
        if (file.Length > 5 * 1024 * 1024)
            return "Розмір файлу не може перевищувати 5 МБ.";

        // Перевірка MIME-типу
        var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif" };
        if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            return "Дозволені формати: JPG, PNG, GIF.";

        // Перевірка розширення (захист від підміни MIME)
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return "Дозволені формати: JPG, PNG, GIF.";

        // Перевірка розмірів зображення
        using var stream = file.OpenReadStream();
        using var image = Image.Load(stream);
        if (image.Width < 300 || image.Height < 300)
            return "Мінімальний розмір зображення: 300x300 пікселів.";
        if (image.Width > 2000 || image.Height > 2000)
            return "Максимальний розмір зображення: 2000x2000 пікселів.";

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
                     && s.EndTime.Value <= DateTime.UtcNow) // Якщо час завершення вже настав або минув
            .ToListAsync();

        if (expiredSessions.Any())
        {
            foreach (var session in expiredSessions)
            {
                session.EndSession(); // Викликаємо доменний метод (рахує гроші, міняє статус)

                // Звільняємо ПК
                if (session.Hardware != null)
                    session.Hardware.Status = CyberZone.Domain.Enums.HardwareStatus.Available;
            }
            await _context.SaveChangesAsync(); // Зберігаємо зміни в базу
        }

        var viewModels = new List<SessionItemViewModel>();

        // Отримуємо часовий пояс України (безпечний спосіб для Windows та Linux/Mac)
        TimeZoneInfo kyivZone;
        try { kyivZone = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time"); } // Для Windows
        catch { kyivZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv"); } // Для Linux/macOS

        // 1. Тягнемо АКТИВНІ та ЗАВЕРШЕНІ сесії
        var sessions = await _context.GamingSessions
            .Include(s => s.Hardware).ThenInclude(h => h.Club)
            .Include(s => s.Tariff)
            .Where(s => s.UserId == userId)
            .ToListAsync();

        viewModels.AddRange(sessions.Select(s =>
        {
            // Переводимо UTC час у Київський для красивого відображення
            var localStartTime = TimeZoneInfo.ConvertTimeFromUtc(s.StartTime, kyivZone);

            return new SessionItemViewModel
            {
                Id = s.Id,
                ClubName = s.Hardware?.Club?.Name ?? "CyberZone Club",
                Address = s.Hardware?.Club?.Address?.ToString() ?? "Адреса не вказана",
                PcNumber = s.Hardware?.PcNumber ?? "N/A",

                // Використовуємо наш локальний час для тексту
                Date = localStartTime.ToString("dd.MM.yyyy"),
                Time = localStartTime.ToString("HH:mm"),

                Duration = s.EndTime.HasValue
                    ? Math.Round((s.EndTime.Value - s.StartTime).TotalHours, 1).ToString() + " год."
                    : "Триває",
                SessionState = s.Status == CyberZone.Domain.Enums.SessionStatus.Active ? "Active" : "Completed",

                // Для JS і сортування залишаємо оригінальний UTC (браузер сам його зрозуміє)
                SortDate = s.StartTime,
                TargetTime = s.Status == CyberZone.Domain.Enums.SessionStatus.Active && s.EndTime.HasValue
                    ? s.EndTime.Value.ToString("yyyy-MM-ddTHH:mm:ssZ")
                    : ""
            };
        }));

        // 2. Тягнемо ПЛАНОВАНІ сесії
        var bookings = await _context.Bookings
            .Include(b => b.Hardware).ThenInclude(h => h.Club)
            .Include(b => b.Tariff)
            .Where(b => b.UserId == userId && b.Status == CyberZone.Domain.Enums.BookingStatus.Pending)
            .ToListAsync();

        viewModels.AddRange(bookings.Select(b =>
        {
            // Переводимо UTC час у Київський для красивого відображення
            var localStartTime = TimeZoneInfo.ConvertTimeFromUtc(b.StartTime, kyivZone);

            return new SessionItemViewModel
            {
                Id = b.Id,
                ClubName = b.Hardware?.Club?.Name ?? "CyberZone Club",
                Address = b.Hardware?.Club?.Address?.ToString() ?? "Адреса не вказана",
                PcNumber = b.Hardware?.PcNumber ?? "N/A",

                // Використовуємо наш локальний час для тексту
                Date = localStartTime.ToString("dd.MM.yyyy"),
                Time = localStartTime.ToString("HH:mm"),

                Duration = Math.Round((b.EndTime - b.StartTime).TotalHours, 1).ToString() + " год.",
                SessionState = "Pending",
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

            if (booking.Hardware != null)
                booking.Hardware.Status = CyberZone.Domain.Enums.HardwareStatus.Busy;

            _context.GamingSessions.Add(newSession);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Sessions");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CancelSession(Guid id)
    {
        // Для бронювань кнопка "Скасувати" просто міняє статус
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
        // А це вже для АКТИВНОЇ сесії (велика картка)
        var session = await _context.GamingSessions
            .Include(s => s.Tariff)
            .Include(s => s.Hardware)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session != null && session.Status == CyberZone.Domain.Enums.SessionStatus.Active)
        {
            session.EndSession(); // Викликаємо твій доменний метод розрахунку

            // Звільняємо ПК
            if (session.Hardware != null)
                session.Hardware.Status = CyberZone.Domain.Enums.HardwareStatus.Available;

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Sessions");
    }

}