using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;
using ResearchManagement.Infrastructure.Data;
using ResearchManagement.Application.Queries.Research;

namespace ResearchManagement.Infrastructure.Repositories
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Review?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Reviews
                .Include(r => r.Research)
                    .ThenInclude(res => res.Authors)
                .Include(r => r.Research)
                    .ThenInclude(res => res.SubmittedBy)
                .Include(r => r.Research)
                    .ThenInclude(res => res.Files.Where(f => f.IsActive))
                .Include(r => r.Reviewer)
                .Include(r => r.ReviewFiles)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }

        public async Task<IEnumerable<Review>> GetByResearchIdAsync(int researchId)
        {
            return await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.ReviewFiles)
                .Where(r => r.ResearchId == researchId && !r.IsDeleted)
                .OrderByDescending(r => r.AssignedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByReviewerIdAsync(string reviewerId)
        {
            return await _context.Reviews
                .Include(r => r.Research)
                    .ThenInclude(res => res.Authors)
                .Include(r => r.Research)
                    .ThenInclude(res => res.SubmittedBy)
                .Include(r => r.ReviewFiles)
                .Where(r => r.ReviewerId == reviewerId && !r.IsDeleted)
                .OrderByDescending(r => r.AssignedDate)
                .ToListAsync();
        }

        public async Task<Review?> GetByResearchAndReviewerAsync(int researchId, string reviewerId)
        {
            return await _context.Reviews
                .Include(r => r.Research)
                .Include(r => r.Reviewer)
                .FirstOrDefaultAsync(r => r.ResearchId == researchId &&
                                        r.ReviewerId == reviewerId &&
                                        !r.IsDeleted);
        }

        public async Task<PagedResult<Review>> GetPagedAsync(
            string userId,
            UserRole userRole,
            string? searchTerm = null,
            string? status = null,
            string? track = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Reviews
                .Include(r => r.Research)
                    .ThenInclude(res => res.Authors)
                .Include(r => r.Research)
                    .ThenInclude(res => res.SubmittedBy)
                .Include(r => r.Reviewer)
                .Where(r => !r.IsDeleted);

            // تطبيق تصفية حسب دور المستخدم
            switch (userRole)
            {
                case UserRole.Reviewer:
                    query = query.Where(r => r.ReviewerId == userId);
                    break;

                case UserRole.TrackManager:
                    // فقط مراجعات البحوث الخاصة بمسار المدير
                    var trackManager = await _context.TrackManagers
                        .FirstOrDefaultAsync(tm => tm.UserId == userId && tm.IsActive);
                    if (trackManager != null)
                    {
                        query = query.Where(r => r.Research.Track == trackManager.Track);
                    }
                    break;

                case UserRole.SystemAdmin:
                    // جميع المراجعات
                    break;

                default:
                    // للأدوار الأخرى، لا يمكن الوصول للمراجعات
                    return new PagedResult<Review>();
            }

            // تطبيق الفلاتر
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(r => r.Research.Title.Contains(searchTerm) ||
                                        r.Research.AbstractAr.Contains(searchTerm) ||
                                        r.Reviewer.FirstName.Contains(searchTerm) ||
                                        r.Reviewer.LastName.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(status))
            {
                switch (status.ToLower())
                {
                    case "pending":
                        query = query.Where(r => !r.IsCompleted);
                        break;
                    case "completed":
                        query = query.Where(r => r.IsCompleted);
                        break;
                    case "overdue":
                        query = query.Where(r => !r.IsCompleted && r.Deadline < DateTime.UtcNow);
                        break;
                }
            }

            if (!string.IsNullOrEmpty(track) && Enum.TryParse<ResearchTrack>(track, out var trackEnum))
            {
                query = query.Where(r => r.Research.Track == trackEnum);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(r => r.AssignedDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(r => r.AssignedDate <= toDate.Value);
            }

            // حساب العدد الإجمالي
            var totalCount = await query.CountAsync();

            // تطبيق الترقيم
            var items = await query
                .OrderByDescending(r => r.AssignedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Review>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        public async Task<IEnumerable<Review>> GetPendingReviewsAsync(string reviewerId)
        {
            return await _context.Reviews
                .Include(r => r.Research)
                    .ThenInclude(res => res.Authors)
                .Include(r => r.Research)
                    .ThenInclude(res => res.Files.Where(f => f.IsActive))
                .Where(r => r.ReviewerId == reviewerId &&
                           !r.IsCompleted &&
                           !r.IsDeleted)
                .OrderBy(r => r.Deadline)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetCompletedReviewsAsync(string reviewerId)
        {
            return await _context.Reviews
                .Include(r => r.Research)
                    .ThenInclude(res => res.Authors)
                .Include(r => r.Reviewer)
                .Where(r => r.ReviewerId == reviewerId &&
                           r.IsCompleted &&
                           !r.IsDeleted)
                .OrderByDescending(r => r.CompletedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetOverdueReviewsAsync(string? reviewerId = null)
        {
            var query = _context.Reviews
                .Include(r => r.Research)
                .Include(r => r.Reviewer)
                .Where(r => !r.IsCompleted &&
                           r.Deadline < DateTime.UtcNow &&
                           !r.IsDeleted);

            if (!string.IsNullOrEmpty(reviewerId))
            {
                query = query.Where(r => r.ReviewerId == reviewerId);
            }

            return await query
                .OrderBy(r => r.Deadline)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByTrackAsync(ResearchTrack track)
        {
            return await _context.Reviews
                .Include(r => r.Research)
                .Include(r => r.Reviewer)
                .Where(r => r.Research.Track == track && !r.IsDeleted)
                .OrderByDescending(r => r.AssignedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByReviewerAndTrackAsync(string reviewerId, ResearchTrack track)
        {
            return await _context.Reviews
                .Include(r => r.Research)
                .Include(r => r.Reviewer)
                .Where(r => r.ReviewerId == reviewerId &&
                           r.Research.Track == track &&
                           !r.IsDeleted)
                .OrderByDescending(r => r.AssignedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByTrackAndDateRangeAsync(ResearchTrack track, DateTime fromDate, DateTime toDate)
        {
            return await _context.Reviews
                .Include(r => r.Research)
                .Include(r => r.Reviewer)
                .Where(r => r.Research.Track == track &&
                           r.AssignedDate >= fromDate &&
                           r.AssignedDate <= toDate &&
                           !r.IsDeleted)
                .OrderByDescending(r => r.AssignedDate)
                .ToListAsync();
        }

        public async Task<int> GetCompletedReviewsCountAsync(int researchId)
        {
            return await _context.Reviews
                .CountAsync(r => r.ResearchId == researchId &&
                                r.IsCompleted &&
                                !r.IsDeleted);
        }

        public async Task<decimal> GetAverageScoreAsync(int researchId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ResearchId == researchId &&
                           r.IsCompleted &&
                           !r.IsDeleted)
                .ToListAsync();

            if (!reviews.Any())
                return 0;

            return reviews.Average(r => r.OverallScore);
        }

        public async Task<ReviewerStatisticsDto> GetReviewerStatisticsAsync(string reviewerId, ResearchTrack? track = null)
        {
            var query = _context.Reviews
                .Include(r => r.Research)
                .Where(r => r.ReviewerId == reviewerId && !r.IsDeleted);

            if (track.HasValue)
            {
                query = query.Where(r => r.Research.Track == track.Value);
            }

            var reviews = await query.ToListAsync();
            var completedReviews = reviews.Where(r => r.IsCompleted).ToList();
            var pendingReviews = reviews.Where(r => !r.IsCompleted).ToList();
            var overdueReviews = pendingReviews.Where(r => r.Deadline < DateTime.UtcNow).ToList();

            return new ReviewerStatisticsDto
            {
                ReviewerId = reviewerId,
                TotalAssigned = reviews.Count,
                CompletedReviews = completedReviews.Count,
                PendingReviews = pendingReviews.Count,
                OverdueReviews = overdueReviews.Count,
                AverageReviewTime = completedReviews.Any() && completedReviews.All(r => r.CompletedDate.HasValue)
                    ? completedReviews.Average(r => (r.CompletedDate!.Value - r.AssignedDate).TotalDays)
                    : 0,
                AverageScore = completedReviews.Any()
                    ? (double)completedReviews.Average(r => r.OverallScore)
                    : 0,
                AcceptanceRate = completedReviews.Any()
                    ? (double)completedReviews.Count(r => r.Decision == ReviewDecision.AcceptAsIs ||
                                                        r.Decision == ReviewDecision.AcceptWithMinorRevisions) /
                      completedReviews.Count * 100
                    : 0,
                LastReviewDate = completedReviews.OrderByDescending(r => r.CompletedDate)
                                               .FirstOrDefault()?.CompletedDate
            };
        }

        public async Task<bool> CanReviewerAccessResearchAsync(string reviewerId, int researchId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.ReviewerId == reviewerId &&
                              r.ResearchId == researchId &&
                              !r.IsDeleted);
        }

        public async Task<IEnumerable<Review>> GetReviewsForDecisionAsync(int researchId)
        {
            return await _context.Reviews
                .Include(r => r.Reviewer)
                .Where(r => r.ResearchId == researchId &&
                           r.IsCompleted &&
                           !r.IsDeleted)
                .OrderByDescending(r => r.CompletedDate)
                .ToListAsync();
        }

        public override async Task DeleteAsync(int id)
        {
            var review = await GetByIdAsync(id);
            if (review != null)
            {
                review.IsDeleted = true;
                review.UpdatedAt = DateTime.UtcNow;
                await UpdateAsync(review);
            }
        }
    }

    // ====================================
    // Infrastructure/Repositories/TrackManagerRepository.cs
    // ====================================

    public class TrackManagerRepository : GenericRepository<TrackManager>, ITrackManagerRepository
    {
        public TrackManagerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<TrackManager?> GetByUserIdAsync(string userId)
        {
            return await _context.TrackManagers
                .Include(tm => tm.User)
                .FirstOrDefaultAsync(tm => tm.UserId == userId &&
                                          tm.IsActive &&
                                          !tm.IsDeleted);
        }

        public async Task<TrackManager?> GetByUserIdAndTrackAsync(string userId, ResearchTrack track)
        {
            return await _context.TrackManagers
                .Include(tm => tm.User)
                .FirstOrDefaultAsync(tm => tm.UserId == userId &&
                                          tm.Track == track &&
                                          tm.IsActive &&
                                          !tm.IsDeleted);
        }

        public async Task<IEnumerable<TrackManager>> GetByTrackAsync(ResearchTrack track)
        {
            return await _context.TrackManagers
                .Include(tm => tm.User)
                .Where(tm => tm.Track == track &&
                            tm.IsActive &&
                            !tm.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> IsUserTrackManagerAsync(string userId, ResearchTrack track)
        {
            return await _context.TrackManagers
                .AnyAsync(tm => tm.UserId == userId &&
                               tm.Track == track &&
                               tm.IsActive &&
                               !tm.IsDeleted);
        }
    }
}