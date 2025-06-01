using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Infrastructure.Data;

namespace ResearchManagement.Infrastructure.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public ReviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Review?> GetByIdAsync(int id)
        {
            return await _context.Reviews.FindAsync(id);
        }

        public async Task<Review?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Reviews
                .Include(r => r.Research)
                    .ThenInclude(res => res.Authors)
                .Include(r => r.Reviewer)
                .Include(r => r.ReviewFiles)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Review>> GetByResearchIdAsync(int researchId)
        {
            return await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.ReviewFiles)
                .Where(r => r.ResearchId == researchId)
                .OrderByDescending(r => r.AssignedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByReviewerIdAsync(string reviewerId)
        {
            return await _context.Reviews
                .Include(r => r.Research)
                    .ThenInclude(res => res.Authors)
                .Include(r => r.ReviewFiles)
                .Where(r => r.ReviewerId == reviewerId)
                .OrderByDescending(r => r.AssignedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetPendingReviewsAsync(string reviewerId)
        {
            return await _context.Reviews
                .Include(r => r.Research)
                    .ThenInclude(res => res.Authors)
                .Where(r => r.ReviewerId == reviewerId && !r.IsCompleted)
                .OrderBy(r => r.Deadline)
                .ToListAsync();
        }

        public async Task AddAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
        }

        public async Task UpdateAsync(Review review)
        {
            _context.Reviews.Update(review);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var review = await GetByIdAsync(id);
            if (review != null)
            {
                review.IsDeleted = true;
                review.UpdatedAt = DateTime.UtcNow;
            }
        }

        public async Task<int> GetCompletedReviewsCountAsync(int researchId)
        {
            return await _context.Reviews
                .CountAsync(r => r.ResearchId == researchId && r.IsCompleted);
        }

        public async Task<decimal> GetAverageScoreAsync(int researchId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ResearchId == researchId && r.IsCompleted)
                .ToListAsync();

            if (!reviews.Any())
                return 0;

            return reviews.Average(r => r.OverallScore);
        }
    }
}
