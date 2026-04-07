using CyberZone.Application.DTOs;
using CyberZone.Application.Interfaces;
using CyberZone.Domain.Entities;
using CyberZone.Domain.Enums;
using CyberZone.Infrastructure.Services;
using CyberZone.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace CyberZone.Tests.Infrastructure.Services;

public class UserServiceUpdateTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly UserService _service;

    public UserServiceUpdateTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        var userStore = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _service = new UserService(_mockUserManager.Object, _mockContext.Object);
    }

    // --- UpdateUserProfileAsync ---

    [Fact]
    public async Task UpdateUserProfileAsync_ValidDto_ReturnsSuccess()
    {
        var user = CreateTestUser();
        SetupUserManager(user);

        var dto = new EditUserProfileDto
        {
            UserId = user.Id.ToString(),
            Email = "updated@test.com",
            FullName = "Updated Name",
            Bio = "Updated bio",
            Phone = "+380997654321",
            Location = "Lviv",
            WebsiteUrl = "https://updated.com",
            ProfileImagePath = "/images/profiles/new.jpg"
        };

        var result = await _service.UpdateUserProfileAsync(dto);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateUserProfileAsync_NonExistentUser_ReturnsFailure()
    {
        _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var dto = new EditUserProfileDto
        {
            UserId = Guid.NewGuid().ToString(),
            Email = "test@test.com"
        };

        var result = await _service.UpdateUserProfileAsync(dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не знайдено");
    }

    [Fact]
    public async Task UpdateUserProfileAsync_UpdatesAllFields()
    {
        var user = CreateTestUser();
        SetupUserManager(user);

        var dto = new EditUserProfileDto
        {
            UserId = user.Id.ToString(),
            Email = "new@email.com",
            FullName = "New Name",
            Bio = "New bio",
            Phone = "+380111111111",
            Location = "Odesa",
            WebsiteUrl = "https://new-site.com",
            ProfileImagePath = "/images/profiles/avatar.png"
        };

        await _service.UpdateUserProfileAsync(dto);

        _mockUserManager.Verify(m => m.UpdateAsync(
            It.Is<User>(u =>
                u.Email == "new@email.com" &&
                u.FullName == "New Name" &&
                u.Bio == "New bio" &&
                u.Phone == "+380111111111" &&
                u.Location == "Odesa" &&
                u.WebsiteUrl == "https://new-site.com" &&
                u.ProfileImagePath == "/images/profiles/avatar.png")),
            Times.Once);
    }

    [Fact]
    public async Task UpdateUserProfileAsync_IdentityFailure_ReturnsFailureWithErrors()
    {
        var user = CreateTestUser();

        _mockUserManager.Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "Duplicate email" }));

        var dto = new EditUserProfileDto
        {
            UserId = user.Id.ToString(),
            Email = "existing@test.com"
        };

        var result = await _service.UpdateUserProfileAsync(dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Duplicate email");
    }

    // --- GetUserProfileAsync ---

    [Fact]
    public async Task GetUserProfileAsync_ReturnsNewFieldsInDto()
    {
        var user = CreateTestUser();
        user.Bio = "My bio";
        user.Phone = "+380999999999";
        user.Location = "Kyiv";
        user.WebsiteUrl = "https://test.com";
        user.ProfileImagePath = "/images/profiles/test.jpg";

        _mockUserManager.Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        var transactions = new List<Transaction>();
        var mockTransactions = MockDbSetHelper.CreateMockDbSet(transactions);
        _mockContext.Setup(c => c.Transactions).Returns(mockTransactions.Object);

        var result = await _service.GetUserProfileAsync(user.Id.ToString());

        result.Should().NotBeNull();
        result!.Bio.Should().Be("My bio");
        result.Phone.Should().Be("+380999999999");
        result.Location.Should().Be("Kyiv");
        result.WebsiteUrl.Should().Be("https://test.com");
        result.ProfileImagePath.Should().Be("/images/profiles/test.jpg");
        result.FullName.Should().Be("Test User");
    }

    [Fact]
    public async Task GetUserProfileAsync_NonExistentUser_ReturnsNull()
    {
        _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var result = await _service.GetUserProfileAsync(Guid.NewGuid().ToString());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserProfileAsync_WithTransactions_ReturnsTransactions()
    {
        var user = CreateTestUser();
        _mockUserManager.Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        var transactions = new List<Transaction>
        {
            new() { UserId = user.Id, Type = TransactionType.TopUp, Amount = 100m, Description = "Top up", TransactionDate = DateTime.UtcNow },
            new() { UserId = user.Id, Type = TransactionType.SessionCharge, Amount = 50m, Description = "Session", TransactionDate = DateTime.UtcNow }
        };
        var mockTransactions = MockDbSetHelper.CreateMockDbSet(transactions);
        _mockContext.Setup(c => c.Transactions).Returns(mockTransactions.Object);

        var result = await _service.GetUserProfileAsync(user.Id.ToString());

        result.Should().NotBeNull();
        result!.Transactions.Should().HaveCount(2);
    }

    private void SetupUserManager(User user)
    {
        _mockUserManager.Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);
    }

    private static User CreateTestUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
            Email = "test@test.com",
            FullName = "Test User"
        };
    }
}
