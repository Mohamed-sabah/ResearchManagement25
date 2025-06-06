using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Application.Interfaces
{
    public interface IResearchRepository : IGenericRepository<Research>
    {
        Task<Research?> GetByIdWithDetailsAsync(int id);
        Task<Research?> GetByIdWithAuthorsAsync(int id);
        Task<Research?> GetByIdWithFilesAsync(int id);
        Task<Research?> GetByIdWithReviewsAsync(int id);
        Task<Research?> GetByIdWithAllDetailsAsync(int id);

        Task<IEnumerable<Research>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Research>> GetByUserIdWithDetailsAsync(string userId);

        Task<IEnumerable<Research>> GetByTrackAsync(ResearchTrack track);
        Task<IEnumerable<Research>> GetByTrackWithDetailsAsync(ResearchTrack track);

        Task<IEnumerable<Research>> GetByStatusAsync(ResearchStatus status);
        Task<IEnumerable<Research>> GetByStatusWithDetailsAsync(ResearchStatus status);

        Task<IEnumerable<Research>> GetByTrackManagerAsync(int trackManagerId);
        Task<IEnumerable<Research>> GetForReviewerAsync(string reviewerId);

        Task<IEnumerable<Research>> GetBySubmissionDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<Research>> GetRecentSubmissionsAsync(int count = 10);

        Task<int> GetCountByStatusAsync(ResearchStatus status);
        Task<int> GetCountByTrackAsync(ResearchTrack track);
        Task<int> GetCountByUserAsync(string userId);

        Task<IEnumerable<Research>> SearchAsync(string searchTerm);
        Task<IEnumerable<Research>> SearchByTitleAsync(string title);
        Task<IEnumerable<Research>> SearchByKeywordsAsync(string keywords);
        Task<IEnumerable<Research>> SearchByAuthorAsync(string authorName);

        Task<bool> HasUserSubmittedResearchAsync(string userId);
        Task<bool> CanUserSubmitResearchAsync(string userId);

        Task<IEnumerable<Research>> GetPendingReviewAssignmentAsync();
        Task<IEnumerable<Research>> GetResearchWithOverdueReviewsAsync();

        Task<decimal> GetAverageReviewScoreAsync(int researchId);
        Task<int> GetCompletedReviewsCountAsync(int researchId);

        Task SoftDeleteAsync(int id, string deletedBy);
        Task RestoreAsync(int id, string restoredBy);

        Task<IEnumerable<Research>> GetDeletedResearchAsync();
        Task<IEnumerable<Research>> GetArchivedResearchAsync();
    }
}