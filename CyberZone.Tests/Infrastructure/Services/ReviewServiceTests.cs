using CyberZone.Application.DTOs;
using CyberZone.Application.Interfaces;
using CyberZone.Domain.Entities;
using CyberZone.Infrastructure.Services;
using CyberZone.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace CyberZone.Tests.Infrastructure.Services;

public class ReviewServiceTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly ReviewService _service;
    private readonly List<Review> _reviews;
    private readonly List<Club> _clubs;

    public ReviewServiceTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        var logger = new Mock<ILogger<ReviewService>>();
        _reviews = [];
        _clubs = [];
        SetupMocks();
        _service = new ReviewService(_mockContext.Object, logger.Object);
    }

    private void SetupMocks()
    {
        var mockReviews = MockDbSetHelper.CreateMockDbSet(_reviews);
        mockReviews.Setup(m => m.Add(It.IsAny<Review>()))
            .Callback<Review>(r => _reviews.Add(r));
        _mockContext.Setup(c => c.Reviews).Returns(mockReviews.Object);

        var mockClubs = MockDbSetHelper.CreateMockDbSet(_clubs);
        _mockContext.Setup(c => c.Clubs).Returns(mockClubs.Object);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    private void AddClub(Guid? id = null)
    {
        var club = new Club
        {
            Id = id ?? Guid.NewGuid(),
            Name = "Test Club"
        };
        _clubs.Add(club);
        // Re-setup mocks to include new data
        var mockClubs = MockDbSetHelper.CreateMockDbSet(_clubs);
        _mockContext.Setup(c => c.Clubs).Returns(mockClubs.Object);
    }

    private void AddExistingReview(Guid userId, Guid clubId)
    {
        _reviews.Add(new Review { UserId = userId, ClubId = clubId, Rating = 5 });
        var mockReviews = MockDbSetHelper.CreateMockDbSet(_reviews);
        mockReviews.Setup(m => m.Add(It.IsAny<Review>()))
            .Callback<Review>(r => _reviews.Add(r));
        _mockContext.Setup(c => c.Reviews).Returns(mockReviews.Object);
    }

    // --- Success ---

    [Fact]
    public async Task AddReviewAsync_ValidData_ReturnsSuccess()
    {
        var clubId = Guid.NewGuid();
        AddClub(clubId);
        var userId = Guid.NewGuid();

        var dto = new CreateReviewDto { ClubId = clubId, Rating = 4, Comment = "Great club!" };

        var result = await _service.AddReviewAsync(userId, dto);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AddReviewAsync_ValidData_AddsReviewToContext()
    {
        var clubId = Guid.NewGuid();
        AddClub(clubId);
        var userId = Guid.NewGuid();

        var dto = new CreateReviewDto { ClubId = clubId, Rating = 5, Comment = "Excellent!" };

        await _service.AddReviewAsync(userId, dto);

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddReviewAsync_NullComment_ReturnsSuccess()
    {
        var clubId = Guid.NewGuid();
        AddClub(clubId);

        var dto = new CreateReviewDto { ClubId = clubId, Rating = 3, Comment = null };

        var result = await _service.AddReviewAsync(Guid.NewGuid(), dto);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateReviewAsync_ValidData_ReturnsSuccessAndUpdatesFields()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var review = new Review
        {
            Id = reviewId,
            UserId = userId,
            ClubId = Guid.NewGuid(),
            Rating = 3,
            Comment = "Old comment",
            CreatedAt = DateTime.UtcNow
        };
        _reviews.Add(review);

        var mockReviews = MockDbSetHelper.CreateMockDbSet(_reviews);
        _mockContext.Setup(c => c.Reviews).Returns(mockReviews.Object);

        // Act
        var result = await _service.UpdateReviewAsync(reviewId, userId, 5, "New comment");

        // Assert
        result.IsSuccess.Should().BeTrue();
        review.Rating.Should().Be(5);
        review.Comment.Should().Be("New comment");
        review.UpdatedAt.Should().NotBeNull();
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateReviewAsync_ReviewNotFoundOrWrongUser_ReturnsFailure()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid(); // User attempting the update
        var actualOwnerId = Guid.NewGuid(); // Actual owner of the review
        var review = new Review
        {
            Id = reviewId,
            UserId = actualOwnerId,
            ClubId = Guid.NewGuid(),
            Rating = 3
        };
        _reviews.Add(review);

        var mockReviews = MockDbSetHelper.CreateMockDbSet(_reviews);
        _mockContext.Setup(c => c.Reviews).Returns(mockReviews.Object);

        // Act
        var result = await _service.UpdateReviewAsync(reviewId, userId, 5, "New comment");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не знайдено або у вас немає прав");
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // --- DeleteReviewAsync ---

    [Fact]
    public async Task DeleteReviewAsync_ValidData_ReturnsSuccessAndRemovesReview()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var review = new Review
        {
            Id = reviewId,
            UserId = userId,
            ClubId = Guid.NewGuid(),
            Rating = 5
        };
        _reviews.Add(review);

        var mockReviews = MockDbSetHelper.CreateMockDbSet(_reviews);
        _mockContext.Setup(c => c.Reviews).Returns(mockReviews.Object);

        // Act
        var result = await _service.DeleteReviewAsync(reviewId, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        mockReviews.Verify(m => m.Remove(review), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteReviewAsync_ReviewNotFoundOrWrongUser_ReturnsFailure()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var mockReviews = MockDbSetHelper.CreateMockDbSet(_reviews);
        _mockContext.Setup(c => c.Reviews).Returns(mockReviews.Object);

        // Act
        var result = await _service.DeleteReviewAsync(reviewId, userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не знайдено або у вас немає прав");
        mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // --- Rating validation ---

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task AddReviewAsync_RatingTooLow_ReturnsFailure(int rating)
    {
        var dto = new CreateReviewDto { ClubId = Guid.NewGuid(), Rating = rating };

        var result = await _service.AddReviewAsync(Guid.NewGuid(), dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("1");
        result.Error.Should().Contain("5");
    }

    [Theory]
    [InlineData(6)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task AddReviewAsync_RatingTooHigh_ReturnsFailure(int rating)
    {
        var dto = new CreateReviewDto { ClubId = Guid.NewGuid(), Rating = rating };

        var result = await _service.AddReviewAsync(Guid.NewGuid(), dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("1");
        result.Error.Should().Contain("5");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task AddReviewAsync_ValidRatingBoundaries_ReturnsSuccess(int rating)
    {
        var clubId = Guid.NewGuid();
        AddClub(clubId);

        var dto = new CreateReviewDto { ClubId = clubId, Rating = rating };

        var result = await _service.AddReviewAsync(Guid.NewGuid(), dto);

        result.IsSuccess.Should().BeTrue();
    }

    // --- Comment validation ---

    [Fact]
    public async Task AddReviewAsync_CommentTooLong_ReturnsFailure()
    {
        var dto = new CreateReviewDto
        {
            ClubId = Guid.NewGuid(),
            Rating = 4,
            Comment = new string('x', 501)
        };

        var result = await _service.AddReviewAsync(Guid.NewGuid(), dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("500");
    }

    [Fact]
    public async Task AddReviewAsync_CommentExactly500Chars_ReturnsSuccess()
    {
        var clubId = Guid.NewGuid();
        AddClub(clubId);

        var dto = new CreateReviewDto
        {
            ClubId = clubId,
            Rating = 4,
            Comment = new string('x', 500)
        };

        var result = await _service.AddReviewAsync(Guid.NewGuid(), dto);

        result.IsSuccess.Should().BeTrue();
    }

    // --- Club not found ---

    [Fact]
    public async Task AddReviewAsync_ClubNotFound_ReturnsFailure()
    {
        // No clubs added
        var dto = new CreateReviewDto { ClubId = Guid.NewGuid(), Rating = 4 };

        var result = await _service.AddReviewAsync(Guid.NewGuid(), dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не знайдено");
    }

    // --- Duplicate review ---

    [Fact]
    public async Task AddReviewAsync_DuplicateReview_ReturnsFailure()
    {
        var clubId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        AddClub(clubId);
        AddExistingReview(userId, clubId);

        var dto = new CreateReviewDto { ClubId = clubId, Rating = 3 };

        var result = await _service.AddReviewAsync(userId, dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("вже");
    }

    [Fact]
    public async Task AddReviewAsync_SameUserDifferentClub_ReturnsSuccess()
    {
        var clubId1 = Guid.NewGuid();
        var clubId2 = Guid.NewGuid();
        var userId = Guid.NewGuid();
        AddClub(clubId1);
        AddClub(clubId2);
        AddExistingReview(userId, clubId1);

        var dto = new CreateReviewDto { ClubId = clubId2, Rating = 5 };

        var result = await _service.AddReviewAsync(userId, dto);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AddReviewAsync_DifferentUserSameClub_ReturnsSuccess()
    {
        var clubId = Guid.NewGuid();
        AddClub(clubId);
        AddExistingReview(Guid.NewGuid(), clubId);

        var dto = new CreateReviewDto { ClubId = clubId, Rating = 4 };

        var result = await _service.AddReviewAsync(Guid.NewGuid(), dto);

        result.IsSuccess.Should().BeTrue();
    }
}
