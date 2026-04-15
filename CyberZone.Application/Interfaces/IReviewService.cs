using CyberZone.Application.Common;
using CyberZone.Application.DTOs;

namespace CyberZone.Application.Interfaces;

public interface IReviewService
{
    Task<Result> AddReviewAsync(Guid userId, CreateReviewDto dto);
    Task<Result> UpdateReviewAsync(Guid reviewId, Guid userId, int rating, string? comment);
    Task<Result> DeleteReviewAsync(Guid reviewId, Guid userId);
}
