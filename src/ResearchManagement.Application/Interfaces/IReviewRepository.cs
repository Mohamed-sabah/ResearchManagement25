using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ResearchManagement.Domain.Entities;

namespace ResearchManagement.Application.Interfaces
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        Task<Review?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Review>> GetByResearchIdAsync(int researchId);
        Task<IEnumerable<Review>> GetByReviewerIdAsync(string reviewerId);
        Task<IEnumerable<Review>> GetPendingReviewsAsync(string reviewerId);
        Task<int> GetCompletedReviewsCountAsync(int researchId);
        Task<decimal> GetAverageScoreAsync(int researchId);
    }
}