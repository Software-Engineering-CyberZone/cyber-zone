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
    public async Task<IActionResult> Sessions()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null || !Guid.TryParse(userIdStr, out Guid userId))
            return RedirectToAction("Login");

        // Дістаємо реальні сесії з бази даних
        var realSessions = await _context.GamingSessions
            .Include(s => s.Hardware) // Підтягуємо інфо про ПК/Консоль
                .ThenInclude(h => h.Club) // В Hardware є прямий зв'язок з Club
            .Include(s => s.Tariff) // Підтягуємо тариф для впевненості (опціонально)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartTime) // Найновіші сесії зверху
            .ToListAsync();

        // Перетворюємо їх у нашу ViewModel
        var sessionViewModels = realSessions.Select(s => new SessionItemViewModel
        {
            Id = s.Id.GetHashCode(), // Або замініть Id в ViewModel на Guid, щоб передавати s.Id

            // Беремо реальну назву клубу
            ClubName = s.Hardware?.Club?.Name ?? "CyberZone Club",

            // Якщо у ValueObject Address перевизначено ToString(), це спрацює ідеально.
            // Якщо ні, напишіть: $"{s.Hardware?.Club?.Address.City}, {s.Hardware?.Club?.Address.Street}"
            Address = s.Hardware?.Club?.Address?.ToString() ?? "Адреса не вказана",

            // Використовуємо реальне поле PcNumber з Hardware (наприклад, "PC-01")
            PcNumber = s.Hardware?.PcNumber ?? "N/A",

            // Форматуємо дати та час
            Date = s.StartTime.ToString("dd.MM.yyyy"),
            Time = s.StartTime.ToString("HH:mm"),

            // Вираховуємо тривалість. Якщо EndTime ще немає (сесія триває), рахуємо до поточного часу
            Duration = s.EndTime.HasValue
                ? Math.Round((s.EndTime.Value - s.StartTime).TotalHours, 1).ToString() + " год."
                : Math.Round((DateTime.UtcNow - s.StartTime).TotalHours, 1).ToString() + " год. (триває)",

            // Визначаємо, чи сесія зараз активна, використовуючи ваш enum SessionStatus.Active
            IsActive = s.Status == CyberZone.Domain.Enums.SessionStatus.Active
        }).ToList();

        return View(sessionViewModels);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> SeedMockSessions()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null || !Guid.TryParse(userIdStr, out Guid userId))
            return RedirectToAction("Login");

        // Перевіряємо, чи вже є сесії, щоб не створити дублікати
        if (await _context.GamingSessions.AnyAsync(s => s.UserId == userId))
        {
            return RedirectToAction("Sessions");
        }

        // 1. Створюємо фейковий Клуб
        var club = new CyberZone.Domain.Entities.Club
        {
            Name = "Cyber Pro Arena",
            CreatedAt = DateTime.UtcNow
        };
        _context.Clubs.Add(club);

        // 2. Створюємо фейкове залізо (ПК)
        var hardware = new CyberZone.Domain.Entities.Hardware
        {
            PcNumber = "PC-20",
            Club = club, // EF сам зв'яже їхні ID
            Status = CyberZone.Domain.Enums.HardwareStatus.Busy,
            CreatedAt = DateTime.UtcNow
        };
        _context.Hardwares.Add(hardware);

        // 3. Створюємо фейковий Тариф
        var tariff = new CyberZone.Domain.Entities.Tariff
        {
            Name = "Standard",
            PricePerHour = 75.00m,
            CreatedAt = DateTime.UtcNow
        };
        club.Tariffs.Add(tariff); // Зв'язуємо тариф з клубом

        // 4. Створюємо фейкові Сесії для поточного юзера
        var sessions = new List<CyberZone.Domain.Entities.GamingSession>
        {
            // Активна сесія (почалася 1 годину тому і досі триває)
            new CyberZone.Domain.Entities.GamingSession
            {
                UserId = userId,
                Hardware = hardware,
                Tariff = tariff,
                StartTime = DateTime.UtcNow.AddHours(-1),
                Status = CyberZone.Domain.Enums.SessionStatus.Active,
                CreatedAt = DateTime.UtcNow
            },
            // Завершена сесія вчора (грав 2 години)
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
            // Ще одна завершена сесія позавчора (грав 3 години)
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

        _context.GamingSessions.AddRange(sessions);

        // Зберігаємо всю цю красу в базу одним махом!
        await _context.SaveChangesAsync();

        // Перекидаємо на сторінку сесій, щоб побачити результат
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

}