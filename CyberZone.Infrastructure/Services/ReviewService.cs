using CyberZone.Application.Common;
using CyberZone.Application.DTOs;
using CyberZone.Application.Interfaces;
using CyberZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberZone.Infrastructure.Services;

public class ReviewService : IReviewService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(IApplicationDbContext context, ILogger<ReviewService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> AddReviewAsync(Guid userId, CreateReviewDto dto)
    {
        _logger.LogInformation("User {UserId} adding review for club {ClubId}", userId, dto.ClubId);

        if (dto.Rating < 1 || dto.Rating > 5)
            return Result.Failure("Рейтинг має бути від 1 до 5.");

        if (dto.Comment is not null && dto.Comment.Length > 500)
            return Result.Failure("Коментар не може перевищувати 500 символів.");

        var clubExists = await _context.Clubs.AnyAsync(c => c.Id == dto.ClubId);
        if (!clubExists)
            return Result.Failure("Клуб не знайдено.");

        var alreadyReviewed = await _context.Reviews
            .AnyAsync(r => r.UserId == userId && r.ClubId == dto.ClubId);
        if (alreadyReviewed)
            return Result.Failure("Ви вже залишили відгук для цього клубу.");

        var review = new Review
        {
            UserId = userId,
            ClubId = dto.ClubId,
            Rating = dto.Rating,
            Comment = dto.Comment
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Review created for club {ClubId} by user {UserId}", dto.ClubId, userId);
        return Result.Success();
    }
}
