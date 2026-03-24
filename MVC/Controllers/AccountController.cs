using CyberZone.Domain.Entities;
using MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CyberZone.Application.Interfaces;

namespace MVC.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IUserService _userService;

    public AccountController(
        UserManager<User> userManager, 
        SignInManager<User> signInManager,
        IUserService userService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userService = userService; 
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
}