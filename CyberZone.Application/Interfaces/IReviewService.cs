using CyberZone.Application.Common;
using CyberZone.Application.DTOs;

namespace CyberZone.Application.Interfaces;

public interface IReviewService
{
    Task<Result> AddReviewAsync(Guid userId, CreateReviewDto dto);
}
