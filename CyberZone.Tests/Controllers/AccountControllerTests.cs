using CyberZone.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using MVC.Controllers;
using MVC.Models;
using System.Security.Claims;

namespace CyberZone.Tests.Controllers;

public class AccountControllerTests
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<SignInManager<User>> _mockSignInManager;
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        var userStore = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _mockSignInManager = new Mock<SignInManager<User>>(
            _mockUserManager.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<User>>().Object,
            null!, null!, null!, null!);

        _controller = new AccountController(_mockUserManager.Object, _mockSignInManager.Object);

        // Setup default HttpContext with unauthenticated user
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    // --- Register GET ---

    [Fact]
    public void Register_Get_WhenNotAuthenticated_ReturnsView()
    {
        var result = _controller.Register();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Register_Get_WhenAuthenticated_RedirectsToCatalog()
    {
        SetAuthenticated();

        var result = _controller.Register();

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Catalog");
        redirect.ControllerName.Should().Be("Home");
    }

    // --- Register POST ---

    [Fact]
    public async Task Register_Post_ValidModel_CreatesUserAndRedirects()
    {
        var model = new RegisterViewModel
        {
            UserName = "newuser",
            FullName = "New User",
            Email = "new@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        _mockUserManager
            .Setup(m => m.CreateAsync(It.IsAny<User>(), model.Password))
            .ReturnsAsync(IdentityResult.Success);

        _mockSignInManager
            .Setup(m => m.SignInAsync(It.IsAny<User>(), false, null))
            .Returns(Task.CompletedTask);

        var result = await _controller.Register(model);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Catalog");
        redirect.ControllerName.Should().Be("Home");
    }

    [Fact]
    public async Task Register_Post_ValidModel_CallsCreateAsync()
    {
        var model = new RegisterViewModel
        {
            UserName = "testuser",
            FullName = "Test User",
            Email = "test@test.com",
            Password = "Pass123!",
            ConfirmPassword = "Pass123!"
        };

        _mockUserManager
            .Setup(m => m.CreateAsync(It.IsAny<User>(), model.Password))
            .ReturnsAsync(IdentityResult.Success);

        _mockSignInManager
            .Setup(m => m.SignInAsync(It.IsAny<User>(), false, null))
            .Returns(Task.CompletedTask);

        await _controller.Register(model);

        _mockUserManager.Verify(m => m.CreateAsync(
            It.Is<User>(u => u.UserName == "testuser" && u.Email == "test@test.com" && u.FullName == "Test User"),
            "Pass123!"), Times.Once);
    }

    [Fact]
    public async Task Register_Post_IdentityFailure_ReturnsViewWithErrors()
    {
        var model = new RegisterViewModel
        {
            UserName = "user",
            FullName = "User",
            Email = "e@t.com",
            Password = "p",
            ConfirmPassword = "p"
        };

        _mockUserManager
            .Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too short" }));

        var result = await _controller.Register(model);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Register_Post_InvalidModelState_ReturnsView()
    {
        _controller.ModelState.AddModelError("Email", "Required");
        var model = new RegisterViewModel();

        var result = await _controller.Register(model);

        result.Should().BeOfType<ViewResult>();
    }

    // --- Login GET ---

    [Fact]
    public void Login_Get_WhenNotAuthenticated_ReturnsView()
    {
        var result = _controller.Login(returnUrl: "/test");

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        _controller.ViewData["ReturnUrl"].Should().Be("/test");
    }

    [Fact]
    public void Login_Get_WhenAuthenticated_RedirectsToCatalog()
    {
        SetAuthenticated();

        var result = _controller.Login(null);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Catalog");
    }

    // --- Login POST ---

    [Fact]
    public async Task Login_Post_ValidCredentials_RedirectsToCatalog()
    {
        var model = new LoginViewModel { Email = "test@test.com", Password = "Pass123!" };
        var user = new User { UserName = "testuser", Email = "test@test.com" };

        _mockUserManager
            .Setup(m => m.FindByEmailAsync(model.Email))
            .ReturnsAsync(user);

        _mockSignInManager
            .Setup(m => m.PasswordSignInAsync("testuser", model.Password, false, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var result = await _controller.Login(model, null);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Catalog");
    }

    [Fact]
    public async Task Login_Post_ValidCredentials_WithReturnUrl_RedirectsToReturnUrl()
    {
        var model = new LoginViewModel { Email = "test@test.com", Password = "Pass123!" };
        var user = new User { UserName = "testuser", Email = "test@test.com" };

        _mockUserManager
            .Setup(m => m.FindByEmailAsync(model.Email))
            .ReturnsAsync(user);

        _mockSignInManager
            .Setup(m => m.PasswordSignInAsync("testuser", model.Password, false, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        // Setup Url helper to consider local URLs valid
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(u => u.IsLocalUrl("/dashboard")).Returns(true);
        _controller.Url = mockUrlHelper.Object;

        var result = await _controller.Login(model, "/dashboard");

        var redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be("/dashboard");
    }

    [Fact]
    public async Task Login_Post_InvalidCredentials_ReturnsViewWithModelError()
    {
        var model = new LoginViewModel { Email = "test@test.com", Password = "WrongPass!" };
        var user = new User { UserName = "testuser", Email = "test@test.com" };

        _mockUserManager
            .Setup(m => m.FindByEmailAsync(model.Email))
            .ReturnsAsync(user);

        _mockSignInManager
            .Setup(m => m.PasswordSignInAsync("testuser", model.Password, false, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        var result = await _controller.Login(model, null);

        result.Should().BeOfType<ViewResult>();
        _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Login_Post_UserNotFound_ReturnsViewWithModelError()
    {
        var model = new LoginViewModel { Email = "nonexistent@test.com", Password = "Pass123!" };

        _mockUserManager
            .Setup(m => m.FindByEmailAsync(model.Email))
            .ReturnsAsync((User?)null);

        var result = await _controller.Login(model, null);

        result.Should().BeOfType<ViewResult>();
        _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Login_Post_InvalidModelState_ReturnsView()
    {
        _controller.ModelState.AddModelError("Email", "Required");
        var model = new LoginViewModel();

        var result = await _controller.Login(model, null);

        result.Should().BeOfType<ViewResult>();
    }

    // --- Logout ---

    [Fact]
    public async Task Logout_CallsSignOutAndRedirectsToIndex()
    {
        _mockSignInManager
            .Setup(m => m.SignOutAsync())
            .Returns(Task.CompletedTask);

        var result = await _controller.Logout();

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Index");
        redirect.ControllerName.Should().Be("Home");
        _mockSignInManager.Verify(m => m.SignOutAsync(), Times.Once);
    }

    // --- ClubPartner GET ---

    [Fact]
    public void ClubPartner_Get_ReturnsView()
    {
        var result = _controller.ClubPartner();

        result.Should().BeOfType<ViewResult>();
    }

    private void SetAuthenticated()
    {
        var identity = new ClaimsIdentity("TestAuth");
        identity.AddClaim(new Claim(ClaimTypes.Name, "testuser"));
        _controller.HttpContext.User = new ClaimsPrincipal(identity);
    }
}
