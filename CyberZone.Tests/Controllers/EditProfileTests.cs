using CyberZone.Application.Common;
using CyberZone.Application.DTOs;
using CyberZone.Application.Interfaces;
using CyberZone.Domain.Entities;
using CyberZone.Infrastructure.Persistence;
using CyberZone.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MVC.Controllers;
using MVC.Models;
using System.Security.Claims;

namespace CyberZone.Tests.Controllers;

public class EditProfileTests
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<SignInManager<User>> _mockSignInManager;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly AccountController _controller;
    private readonly User _testUser;

    public EditProfileTests()
    {
        var userStore = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _mockSignInManager = new Mock<SignInManager<User>>(
            _mockUserManager.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<User>>().Object,
            null!, null!, null!, null!);

        _mockUserService = new Mock<IUserService>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockEnvironment.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

        var mockDbContext = new Mock<CyberZoneDbContext>(
            new DbContextOptionsBuilder<CyberZoneDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        var mockPaymentService = new Mock<PaymentService>(
            mockDbContext.Object,
            new Mock<ILogger<PaymentService>>().Object);

        _controller = new AccountController(
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _mockUserService.Object,
            mockPaymentService.Object,
            mockDbContext.Object,
            _mockEnvironment.Object);

        _testUser = new User
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
            Email = "test@test.com",
            FullName = "Test User",
            Bio = "Test bio",
            Phone = "+380991234567",
            Location = "Kyiv, Ukraine",
            WebsiteUrl = "https://example.com",
            ProfileImagePath = "/images/profiles/old.jpg"
        };

        SetAuthenticated(_testUser.Id.ToString());
        SetupTempData();
    }

    // --- EditProfile GET ---

    [Fact]
    public async Task EditProfile_GET_AuthenticatedUser_ReturnsViewWithPopulatedModel()
    {
        _mockUserManager.Setup(m => m.FindByIdAsync(_testUser.Id.ToString()))
            .ReturnsAsync(_testUser);

        var result = await _controller.EditProfile();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<EditProfileViewModel>().Subject;
        model.Email.Should().Be("test@test.com");
        model.FullName.Should().Be("Test User");
        model.Bio.Should().Be("Test bio");
        model.Phone.Should().Be("+380991234567");
        model.Location.Should().Be("Kyiv, Ukraine");
        model.WebsiteUrl.Should().Be("https://example.com");
        model.ExistingProfileImagePath.Should().Be("/images/profiles/old.jpg");
    }

    [Fact]
    public async Task EditProfile_GET_UnauthenticatedUser_RedirectsToLogin()
    {
        SetUnauthenticated();

        var result = await _controller.EditProfile();

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Login");
    }

    [Fact]
    public async Task EditProfile_GET_UserNotFound_ReturnsNotFound()
    {
        _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var result = await _controller.EditProfile();

        result.Should().BeOfType<NotFoundResult>();
    }

    // --- EditProfile POST ---

    [Fact]
    public async Task EditProfile_POST_ValidModel_NoImage_RedirectsToProfile()
    {
        var model = CreateValidModel();

        _mockUserManager.Setup(m => m.FindByIdAsync(_testUser.Id.ToString()))
            .ReturnsAsync(_testUser);

        _mockUserService.Setup(s => s.UpdateUserProfileAsync(It.IsAny<EditUserProfileDto>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.EditProfile(model);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Profile");
    }

    [Fact]
    public async Task EditProfile_POST_InvalidModelState_ReturnsViewWithModel()
    {
        _controller.ModelState.AddModelError("Email", "Required");
        var model = new EditProfileViewModel { Email = "" };

        _mockUserManager.Setup(m => m.FindByIdAsync(_testUser.Id.ToString()))
            .ReturnsAsync(_testUser);

        var result = await _controller.EditProfile(model);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<EditProfileViewModel>();
    }

    [Fact]
    public async Task EditProfile_POST_ValidModel_CallsUpdateUserProfileAsync()
    {
        var model = CreateValidModel();
        model.FullName = "Updated Name";

        _mockUserManager.Setup(m => m.FindByIdAsync(_testUser.Id.ToString()))
            .ReturnsAsync(_testUser);

        _mockUserService.Setup(s => s.UpdateUserProfileAsync(It.IsAny<EditUserProfileDto>()))
            .ReturnsAsync(Result.Success());

        await _controller.EditProfile(model);

        _mockUserService.Verify(s => s.UpdateUserProfileAsync(
            It.Is<EditUserProfileDto>(dto =>
                dto.UserId == _testUser.Id.ToString() &&
                dto.Email == "test@test.com" &&
                dto.FullName == "Updated Name")),
            Times.Once);
    }

    [Fact]
    public async Task EditProfile_POST_ServiceReturnsFailure_ReturnsViewWithError()
    {
        var model = CreateValidModel();

        _mockUserManager.Setup(m => m.FindByIdAsync(_testUser.Id.ToString()))
            .ReturnsAsync(_testUser);

        _mockUserService.Setup(s => s.UpdateUserProfileAsync(It.IsAny<EditUserProfileDto>()))
            .ReturnsAsync(Result.Failure("Update failed"));

        var result = await _controller.EditProfile(model);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task EditProfile_POST_ImageUpload_InvalidFormat_ReturnsError()
    {
        var model = CreateValidModel();
        model.ProfileImage = CreateMockFormFile("virus.exe", "application/x-msdownload", 1024);

        _mockUserManager.Setup(m => m.FindByIdAsync(_testUser.Id.ToString()))
            .ReturnsAsync(_testUser);

        var result = await _controller.EditProfile(model);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        _controller.ModelState.ContainsKey("ProfileImage").Should().BeTrue();
    }

    [Fact]
    public async Task EditProfile_POST_ImageUpload_OversizedFile_ReturnsError()
    {
        var model = CreateValidModel();
        model.ProfileImage = CreateMockFormFile("photo.jpg", "image/jpeg", 6 * 1024 * 1024);

        _mockUserManager.Setup(m => m.FindByIdAsync(_testUser.Id.ToString()))
            .ReturnsAsync(_testUser);

        var result = await _controller.EditProfile(model);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        _controller.ModelState.ContainsKey("ProfileImage").Should().BeTrue();
    }

    [Fact]
    public async Task EditProfile_POST_NullImage_KeepsExistingPath()
    {
        var model = CreateValidModel();
        model.ProfileImage = null;

        _mockUserManager.Setup(m => m.FindByIdAsync(_testUser.Id.ToString()))
            .ReturnsAsync(_testUser);

        _mockUserService.Setup(s => s.UpdateUserProfileAsync(It.IsAny<EditUserProfileDto>()))
            .ReturnsAsync(Result.Success());

        await _controller.EditProfile(model);

        _mockUserService.Verify(s => s.UpdateUserProfileAsync(
            It.Is<EditUserProfileDto>(dto =>
                dto.ProfileImagePath == "/images/profiles/old.jpg")),
            Times.Once);
    }

    [Fact]
    public async Task EditProfile_POST_BioIsSanitized()
    {
        var model = CreateValidModel();
        model.Bio = "<script>alert('xss')</script>";

        _mockUserManager.Setup(m => m.FindByIdAsync(_testUser.Id.ToString()))
            .ReturnsAsync(_testUser);

        _mockUserService.Setup(s => s.UpdateUserProfileAsync(It.IsAny<EditUserProfileDto>()))
            .ReturnsAsync(Result.Success());

        await _controller.EditProfile(model);

        _mockUserService.Verify(s => s.UpdateUserProfileAsync(
            It.Is<EditUserProfileDto>(dto =>
                dto.Bio != null &&
                !dto.Bio.Contains("<script>") &&
                dto.Bio.Contains("&lt;script&gt;"))),
            Times.Once);
    }

    [Fact]
    public async Task EditProfile_POST_UnauthenticatedUser_RedirectsToLogin()
    {
        SetUnauthenticated();
        var model = CreateValidModel();

        var result = await _controller.EditProfile(model);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Login");
    }

    [Fact]
    public async Task EditProfile_POST_ImageUpload_InvalidExtension_ReturnsError()
    {
        var model = CreateValidModel();
        // MIME is image/jpeg but extension is .exe — should fail extension check
        model.ProfileImage = CreateMockFormFile("photo.bmp", "image/jpeg", 1024);

        _mockUserManager.Setup(m => m.FindByIdAsync(_testUser.Id.ToString()))
            .ReturnsAsync(_testUser);

        var result = await _controller.EditProfile(model);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        _controller.ModelState.ContainsKey("ProfileImage").Should().BeTrue();
    }

    // --- Helpers ---

    private static EditProfileViewModel CreateValidModel()
    {
        return new EditProfileViewModel
        {
            Email = "test@test.com",
            FullName = "Test User",
            Bio = "A short bio",
            Phone = "+380991234567",
            Location = "Kyiv",
            WebsiteUrl = "https://example.com",
            ProfileImage = null
        };
    }

    private static IFormFile CreateMockFormFile(string fileName, string contentType, long length)
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.ContentType).Returns(contentType);
        fileMock.Setup(f => f.Length).Returns(length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[Math.Min(length, 1024)]));
        return fileMock.Object;
    }

    private void SetAuthenticated(string userId)
    {
        var identity = new ClaimsIdentity("TestAuth");
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
        identity.AddClaim(new Claim(ClaimTypes.Name, "testuser"));
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    private void SetUnauthenticated()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity())
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        SetupTempData();
    }

    private void SetupTempData()
    {
        _controller.TempData = new TempDataDictionary(
            _controller.HttpContext ?? new DefaultHttpContext(),
            new Mock<ITempDataProvider>().Object);
    }
}
