using CyberZone.Domain.Entities;
using MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;

namespace MVC.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    // Впроваджуємо залежності Identity через конструктор
    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // ================= РЕЄСТРАЦІЯ =================

    [HttpGet]
    public IActionResult Register()
    {
        // Якщо юзер вже залогінений - йому не треба на сторінку реєстрації
        if (User.Identity!.IsAuthenticated) return RedirectToAction("Catalog", "Home");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            // 1. Створюємо об'єкт користувача
            var user = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                // Якщо у вашій сутності User є поле FullName, розкоментуй наступний рядок:
                // FullName = model.FullName 
            };

            // 2. Зберігаємо в базу (Identity автоматично хешує пароль)
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // 3. Автоматично логінимо користувача після успішної реєстрації
                await _signInManager.SignInAsync(user, isPersistent: false);

                // 4. Перекидаємо на сторінку каталогу
                return RedirectToAction("Catalog", "Home");
            }

            // Якщо є помилки (наприклад, такий Email вже є), додаємо їх у ModelState
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // Якщо щось пішло не так, повертаємо форму з введеними даними та помилками
        return View(model);
    }

    // ================= ЛОГІН =================

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
            // Identity стандартно логінить по UserName. 
            // Оскільки в нашій формі логіну поле називається Email, ми спочатку шукаємо юзера за Email:
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                // Перевіряємо пароль і логінимо
                var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, isPersistent: false, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Якщо є ReturnUrl (користувач хотів кудись зайти, але його не пустило), повертаємо туди
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    // Інакше - на каталог
                    return RedirectToAction("Catalog", "Home");
                }
            }

            // Щоб не підказувати хакерам, що саме не так (логін чи пароль), виводимо загальну помилку
            ModelState.AddModelError(string.Empty, "Невірний email або пароль.");
        }

        return View(model);
    }

    // ================= ВИХІД =================

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        // Після виходу кидаємо на головну сторінку (Welcome)
        return RedirectToAction("Index", "Home");
    }
}