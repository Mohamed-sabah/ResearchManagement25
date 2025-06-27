using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Queries.Research;
using ResearchManagement.Domain.Entities;


using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Application.Interfaces
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        // Basic CRUD operations
        Task<Review?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Review>> GetByResearchIdAsync(int researchId);
        Task<IEnumerable<Review>> GetByReviewerIdAsync(string reviewerId);
        Task<Review?> GetByResearchAndReviewerAsync(int researchId, string reviewerId);

        // Search and filtering
        Task<PagedResult<Review>> GetPagedAsync(
            string userId,
            UserRole userRole,
            string? searchTerm = null,
            string? status = null,
            string? track = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int page = 1,
            int pageSize = 10);

        // Reviewer specific
        Task<IEnumerable<Review>> GetPendingReviewsAsync(string reviewerId);
        Task<IEnumerable<Review>> GetCompletedReviewsAsync(string reviewerId);
        Task<IEnumerable<Review>> GetOverdueReviewsAsync(string? reviewerId = null);

        // Track Manager specific
        Task<IEnumerable<Review>> GetByTrackAsync(ResearchTrack track);
        Task<IEnumerable<Review>> GetByReviewerAndTrackAsync(string reviewerId, ResearchTrack track);
        Task<IEnumerable<Review>> GetByTrackAndDateRangeAsync(ResearchTrack track, DateTime fromDate, DateTime toDate);

        // Statistics
        Task<int> GetCompletedReviewsCountAsync(int researchId);
        Task<decimal> GetAverageScoreAsync(int researchId);
        Task<ReviewerStatisticsDto> GetReviewerStatisticsAsync(string reviewerId, ResearchTrack? track = null);

        // Decision support
        Task<bool> CanReviewerAccessResearchAsync(string reviewerId, int researchId);
        Task<IEnumerable<Review>> GetReviewsForDecisionAsync(int researchId);
    }

    public interface ITrackManagerRepository : IGenericRepository<TrackManager>
    {
        Task<TrackManager?> GetByUserIdAsync(string userId);
        Task<TrackManager?> GetByUserIdAndTrackAsync(string userId, ResearchTrack track);
        Task<IEnumerable<TrackManager>> GetByTrackAsync(ResearchTrack track);
        Task<bool> IsUserTrackManagerAsync(string userId, ResearchTrack track);
    }

    public interface ITrackReviewerRepository : IGenericRepository<TrackReviewer>
    {
        Task<IEnumerable<TrackReviewer>> GetByTrackAsync(ResearchTrack track);
        Task<IEnumerable<TrackReviewer>> GetByReviewerIdAsync(string reviewerId);
        Task<TrackReviewer?> GetByTrackAndReviewerAsync(ResearchTrack track, string reviewerId);
        Task<bool> IsReviewerInTrackAsync(string reviewerId, ResearchTrack track);
    }

    public interface IResearchStatusHistoryRepository : IGenericRepository<ResearchStatusHistory>
    {
        Task<IEnumerable<ResearchStatusHistory>> GetByResearchIdAsync(int researchId);
        Task<ResearchStatusHistory?> GetLatestByResearchIdAsync(int researchId);
    }
}
