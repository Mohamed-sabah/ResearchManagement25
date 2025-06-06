using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;
using ResearchManagement.Infrastructure.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ResearchManagement.Infrastructure.Repositories
{
    public class ResearchRepository : GenericRepository<Research>, IResearchRepository
    {
        private readonly ILogger<ResearchRepository> _logger;

        public ResearchRepository(ApplicationDbContext context, ILogger<ResearchRepository> logger)
            : base(context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Get Methods with Details

        public async Task<Research?> GetByIdWithDetailsAsync(int id)
        {
            try
            {
                _logger.LogInformation("استرجاع البحث {ResearchId} مع التفاصيل", id);

                return await _dbSet
                    .Include(r => r.SubmittedBy)
                    .Include(r => r.Authors)
                    .Include(r => r.Files.Where(f => f.IsActive))
                    .Include(r => r.Reviews)
                        .ThenInclude(rv => rv.Reviewer)
                    .Include(r => r.StatusHistory)
                        .ThenInclude(sh => sh.ChangedBy)
                    .Include(r => r.AssignedTrackManager)
                        .ThenInclude(tm => tm!.User)
                    .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في استرجاع البحوث الحديثة");
                throw new InvalidOperationException($"خطأ في استرجاع البحوث الحديثة: {ex.Message}", ex);
            }
        }

        #endregion

        #region Count Methods

        public async Task<int> GetCountByStatusAsync(ResearchStatus status)
        {
            try
            {
                return await _dbSet
                    .CountAsync(r => r.Status == status && !r.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في عد البحوث بالحالة {Status}", status);
                throw new InvalidOperationException($"خطأ في عد البحوث بالحالة: {ex.Message}", ex);
            }
        }

        public async Task<int> GetCountByTrackAsync(ResearchTrack track)
        {
            try
            {
                return await _dbSet
                    .CountAsync(r => r.Track == track && !r.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في عد البحوث بالتراك {Track}", track);
                throw new InvalidOperationException($"خطأ في عد البحوث بالتراك: {ex.Message}", ex);
            }
        }

        public async Task<int> GetCountByUserAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    throw new ArgumentException("معرف المستخدم مطلوب", nameof(userId));

                return await _dbSet
                    .CountAsync(r => r.SubmittedById == userId && !r.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في عد بحوث المستخدم {UserId}", userId);
                throw new InvalidOperationException($"خطأ في عد بحوث المستخدم: {ex.Message}", ex);
            }
        }

        #endregion

        #region Search Methods

        public async Task<IEnumerable<Research>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return new List<Research>();

                var term = searchTerm.Trim().ToLower();

                return await _dbSet
                    .Where(r => !r.IsDeleted && (
                        r.Title.ToLower().Contains(term) ||
                        r.TitleEn!.ToLower().Contains(term) ||
                        r.AbstractAr.ToLower().Contains(term) ||
                        r.AbstractEn!.ToLower().Contains(term) ||
                        r.Keywords!.ToLower().Contains(term) ||
                        r.KeywordsEn!.ToLower().Contains(term) ||
                        r.Authors.Any(a =>
                            a.FirstName.ToLower().Contains(term) ||
                            a.LastName.ToLower().Contains(term) ||
                            a.FirstNameEn!.ToLower().Contains(term) ||
                            a.LastNameEn!.ToLower().Contains(term)
                        )
                    ))
                    .Include(r => r.SubmittedBy)
                    .Include(r => r.Authors.OrderBy(a => a.Order))
                    .OrderByDescending(r => r.SubmissionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في البحث عن {SearchTerm}", searchTerm);
                throw new InvalidOperationException($"خطأ في البحث: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Research>> SearchByTitleAsync(string title)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title))
                    return new List<Research>();

                var term = title.Trim().ToLower();

                return await _dbSet
                    .Where(r => !r.IsDeleted && (
                        r.Title.ToLower().Contains(term) ||
                        r.TitleEn!.ToLower().Contains(term)
                    ))
                    .Include(r => r.SubmittedBy)
                    .Include(r => r.Authors.OrderBy(a => a.Order))
                    .OrderByDescending(r => r.SubmissionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في البحث بالعنوان {Title}", title);
                throw new InvalidOperationException($"خطأ في البحث بالعنوان: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Research>> SearchByKeywordsAsync(string keywords)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keywords))
                    return new List<Research>();

                var term = keywords.Trim().ToLower();

                return await _dbSet
                    .Where(r => !r.IsDeleted && (
                        r.Keywords!.ToLower().Contains(term) ||
                        r.KeywordsEn!.ToLower().Contains(term)
                    ))
                    .Include(r => r.SubmittedBy)
                    .Include(r => r.Authors.OrderBy(a => a.Order))
                    .OrderByDescending(r => r.SubmissionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في البحث بالكلمات المفتاحية {Keywords}", keywords);
                throw new InvalidOperationException($"خطأ في البحث بالكلمات المفتاحية: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Research>> SearchByAuthorAsync(string authorName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(authorName))
                    return new List<Research>();

                var term = authorName.Trim().ToLower();

                return await _dbSet
                    .Where(r => !r.IsDeleted &&
                        r.Authors.Any(a =>
                            a.FirstName.ToLower().Contains(term) ||
                            a.LastName.ToLower().Contains(term) ||
                            a.FirstNameEn!.ToLower().Contains(term) ||
                            a.LastNameEn!.ToLower().Contains(term) ||
                            (a.FirstName + " " + a.LastName).ToLower().Contains(term) ||
                            (a.FirstNameEn + " " + a.LastNameEn).ToLower().Contains(term)
                        ))
                    .Include(r => r.SubmittedBy)
                    .Include(r => r.Authors.OrderBy(a => a.Order))
                    .OrderByDescending(r => r.SubmissionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في البحث بالمؤلف {AuthorName}", authorName);
                throw new InvalidOperationException($"خطأ في البحث بالمؤلف: {ex.Message}", ex);
            }
        }

        #endregion

        #region Validation Methods

        public async Task<bool> HasUserSubmittedResearchAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return false;

                return await _dbSet
                    .AnyAsync(r => r.SubmittedById == userId && !r.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في فحص تقديم المستخدم {UserId} للبحوث", userId);
                throw new InvalidOperationException($"خطأ في فحص تقديم البحوث: {ex.Message}", ex);
            }
        }

        public async Task<bool> CanUserSubmitResearchAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return false;

                // فحص عدد البحوث المعلقة للمستخدم
                var pendingResearchCount = await _dbSet
                    .CountAsync(r => r.SubmittedById == userId &&
                                    !r.IsDeleted &&
                                    (r.Status == ResearchStatus.Submitted ||
                                     r.Status == ResearchStatus.UnderReview ||
                                     r.Status == ResearchStatus.UnderEvaluation ||
                                     r.Status == ResearchStatus.RequiresMinorRevisions ||
                                     r.Status == ResearchStatus.RequiresMajorRevisions));

                // السماح بحد أقصى 3 بحوث معلقة
                return pendingResearchCount < 3;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في فحص إمكانية تقديم المستخدم {UserId} للبحوث", userId);
                throw new InvalidOperationException($"خطأ في فحص إمكانية تقديم البحوث: {ex.Message}", ex);
            }
        }

        #endregion

        #region Review Management

        public async Task<IEnumerable<Research>> GetPendingReviewAssignmentAsync()
        {
            try
            {
                return await _dbSet
                    .Where(r => !r.IsDeleted &&
                               r.Status == ResearchStatus.Submitted)
                    .Include(r => r.SubmittedBy)
                    .Include(r => r.Authors.OrderBy(a => a.Order))
                    .OrderBy(r => r.SubmissionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في استرجاع البحوث المعلقة للتوزيع");
                throw new InvalidOperationException($"خطأ في استرجاع البحوث المعلقة للتوزيع: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Research>> GetResearchWithOverdueReviewsAsync()
        {
            try
            {
                var currentDate = DateTime.UtcNow;

                return await _dbSet
                    .Where(r => !r.IsDeleted &&
                               r.Reviews.Any(rv => !rv.IsCompleted && rv.Deadline < currentDate))
                    .Include(r => r.SubmittedBy)
                    .Include(r => r.Authors.OrderBy(a => a.Order))
                    .Include(r => r.Reviews.Where(rv => !rv.IsCompleted && rv.Deadline < currentDate))
                        .ThenInclude(rv => rv.Reviewer)
                    .OrderBy(r => r.Reviews.Min(rv => rv.Deadline))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في استرجاع البحوث ذات المراجعات المتأخرة");
                throw new InvalidOperationException($"خطأ في استرجاع البحوث ذات المراجعات المتأخرة: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetAverageReviewScoreAsync(int researchId)
        {
            try
            {
                var completedReviews = await _context.Reviews
                    .Where(r => r.ResearchId == researchId && r.IsCompleted)
                    .ToListAsync();

                if (!completedReviews.Any())
                    return 0;

                return completedReviews.Average(r => r.OverallScore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حساب متوسط درجات البحث {ResearchId}", researchId);
                throw new InvalidOperationException($"خطأ في حساب متوسط درجات البحث: {ex.Message}", ex);
            }
        }

        public async Task<int> GetCompletedReviewsCountAsync(int researchId)
        {
            try
            {
                return await _context.Reviews
                    .CountAsync(r => r.ResearchId == researchId && r.IsCompleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في عد المراجعات المكتملة للبحث {ResearchId}", researchId);
                throw new InvalidOperationException($"خطأ في عد المراجعات المكتملة: {ex.Message}", ex);
            }
        }

        #endregion

        #region Soft Delete and Restore

        public async Task SoftDeleteAsync(int id, string deletedBy)
        {
            try
            {
                var research = await GetByIdAsync(id);
                if (research != null && !research.IsDeleted)
                {
                    research.IsDeleted = true;
                    research.UpdatedAt = DateTime.UtcNow;
                    research.UpdatedBy = deletedBy;

                    _logger.LogInformation("تم حذف البحث {ResearchId} بواسطة {UserId}", id, deletedBy);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف البحث {ResearchId}", id);
                throw new InvalidOperationException($"خطأ في حذف البحث: {ex.Message}", ex);
            }
        }

        public async Task RestoreAsync(int id, string restoredBy)
        {
            try
            {
                var research = await _dbSet
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (research != null && research.IsDeleted)
                {
                    research.IsDeleted = false;
                    research.UpdatedAt = DateTime.UtcNow;
                    research.UpdatedBy = restoredBy;

                    _logger.LogInformation("تم استرجاع البحث {ResearchId} بواسطة {UserId}", id, restoredBy);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في استرجاع البحث {ResearchId}", id);
                throw new InvalidOperationException($"خطأ في استرجاع البحث: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Research>> GetDeletedResearchAsync()
        {
            try
            {
                return await _dbSet
                    .IgnoreQueryFilters()
                    .Where(r => r.IsDeleted)
                    .Include(r => r.SubmittedBy)
                    .Include(r => r.Authors.OrderBy(a => a.Order))
                    .OrderByDescending(r => r.UpdatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في استرجاع البحوث المحذوفة");
                throw new InvalidOperationException($"خطأ في استرجاع البحوث المحذوفة: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Research>> GetArchivedResearchAsync()
        {
            try
            {
                // البحوث المؤرشفة هي التي تم قبولها أو رفضها نهائياً
                return await _dbSet
                    .Where(r => !r.IsDeleted &&
                               (r.Status == ResearchStatus.Accepted ||
                                r.Status == ResearchStatus.Rejected ||
                                r.Status == ResearchStatus.Withdrawn))
                    .Include(r => r.SubmittedBy)
                    .Include(r => r.Authors.OrderBy(a => a.Order))
                    .OrderByDescending(r => r.DecisionDate ?? r.UpdatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في استرجاع البحوث المؤرشفة");
                throw new InvalidOperationException($"خطأ في استرجاع البحوث المؤرشفة: {ex.Message}", ex);
            }
        }

        #endregion

        #region Override Base Methods

        public override async Task<Research> AddAsync(Research entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                // تحديد القيم الافتراضية
                entity.SubmissionDate = entity.SubmissionDate == default ? DateTime.UtcNow : entity.SubmissionDate;
                entity.CreatedAt = entity.CreatedAt == default ? DateTime.UtcNow : entity.CreatedAt;
                entity.Status = entity.Status == default ? ResearchStatus.Submitted : entity.Status;

                _logger.LogInformation("إضافة بحث جديد: {Title}", entity.Title);

                var result = await base.AddAsync(entity);

                _logger.LogInformation("تم إضافة البحث بنجاح");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إضافة البحث: {Title}", entity?.Title);
                throw new InvalidOperationException($"خطأ في إضافة البحث: {ex.Message}", ex);
            }
        }

        public override async Task UpdateAsync(Research entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                entity.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("تحديث البحث {ResearchId}: {Title}", entity.Id, entity.Title);

                await base.UpdateAsync(entity);

                _logger.LogInformation("تم تحديث البحث {ResearchId} بنجاح", entity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث البحث {ResearchId}", entity?.Id);
                throw new InvalidOperationException($"خطأ في تحديث البحث: {ex.Message}", ex);
            }
        }

        public override async Task DeleteAsync(Research entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                // استخدام Soft Delete بدلاً من الحذف الفعلي
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("حذف البحث {ResearchId}: {Title}", entity.Id, entity.Title);

                await UpdateAsync(entity);

                _logger.LogInformation("تم حذف البحث {ResearchId} بنجاح", entity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف البحث {ResearchId}", entity?.Id);
                throw new InvalidOperationException($"خطأ في حذف البحث: {ex.Message}", ex);
            }
        }

        #endregion
    }
}
LogError(ex, "خطأ في استرجاع البحث {ResearchId} مع التفاصيل", id);
throw new InvalidOperationException($"خطأ في استرجاع البحث: {ex.Message}", ex);
            }
        }

        public async Task<Research?> GetByIdWithAuthorsAsync(int id)
{
    try
    {
        return await _dbSet
            .Include(r => r.SubmittedBy)
            .Include(r => r.Authors.OrderBy(a => a.Order))
                .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "خطأ في استرجاع البحث {ResearchId} مع المؤلفين", id);
        throw new InvalidOperationException($"خطأ في استرجاع البحث مع المؤلفين: {ex.Message}", ex);
    }
}

public async Task<Research?> GetByIdWithFilesAsync(int id)
{
    try
    {
        return await _dbSet
            .Include(r => r.SubmittedBy)
            .Include(r => r.Files.Where(f => f.IsActive))
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "خطأ في استرجاع البحث {ResearchId} مع الملفات", id);
        throw new InvalidOperationException($"خطأ في استرجاع البحث مع الملفات: {ex.Message}", ex);
    }
}

public async Task<Research?> GetByIdWithReviewsAsync(int id)
{
    try
    {
        return await _dbSet
            .Include(r => r.SubmittedBy)
            .Include(r => r.Reviews)
                .ThenInclude(rv => rv.Reviewer)
            .Include(r => r.Reviews)
                .ThenInclude(rv => rv.ReviewFiles)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "خطأ في استرجاع البحث {ResearchId} مع المراجعات", id);
        throw new InvalidOperationException($"خطأ في استرجاع البحث مع المراجعات: {ex.Message}", ex);
    }
}

public async Task<Research?> GetByIdWithAllDetailsAsync(int id)
{
    try
    {
        return await _dbSet
            .Include(r => r.SubmittedBy)
            .Include(r => r.Authors.OrderBy(a => a.Order))
                .ThenInclude(a => a.User)
            .Include(r => r.Files.Where(f => f.IsActive))
            .Include(r => r.Reviews)
                .ThenInclude(rv => rv.Reviewer)
            .Include(r => r.Reviews)
                .ThenInclude(rv => rv.ReviewFiles)
            .Include(r => r.StatusHistory.OrderByDescending(sh => sh.ChangedAt))
                .ThenInclude(sh => sh.ChangedBy)
            .Include(r => r.AssignedTrackManager)
                .ThenInclude(tm => tm!.User)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "خطأ في استرجاع البحث {ResearchId} مع جميع التفاصيل", id);
        throw new InvalidOperationException($"خطأ في استرجاع البحث مع جميع التفاصيل: {ex.Message}", ex);
    }
}

#endregion

#region Get by User

public async Task<IEnumerable<Research>> GetByUserIdAsync(string userId)
{
    try
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("معرف المستخدم مطلوب", nameof(userId));

        _logger.LogInformation("استرجاع بحوث المستخدم {UserId}", userId);

        return await _dbSet
            .Where(r => r.SubmittedById == userId && !r.IsDeleted)
            .Include(r => r.Authors.OrderBy(a => a.Order))
            .Include(r => r.Files.Where(f => f.IsActive))
            .OrderByDescending(r => r.SubmissionDate)
            .ToListAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "خطأ في استرجاع بحوث المستخدم {UserId}", userId);
        throw new InvalidOperationException($"خطأ في استرجاع بحوث المستخدم: {ex.Message}", ex);
    }
}

public async Task<IEnumerable<Research>> GetByUserIdWithDetailsAsync(string userId)
{
    try
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("معرف المستخدم مطلوب", nameof(userId));

        return await _dbSet
            .Where(r => r.SubmittedById == userId && !r.IsDeleted)
            .Include(r => r.SubmittedBy)
            .Include(r => r.Authors.OrderBy(a => a.Order))
            .Include(r => r.Files.Where(f => f.IsActive))
            .Include(r => r.Reviews)
            .OrderByDescending(r => r.SubmissionDate)
            .ToListAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "خطأ في استرجاع بحوث المستخدم {UserId} مع التفاصيل", userId);
        throw new InvalidOperationException($"خطأ في استرجاع بحوث المستخدم مع التفاصيل: {ex.Message}", ex);
    }
}

#endregion

#region Get by Track

public async Task<IEnumerable<Research>> GetByTrackAsync(ResearchTrack track)
{
    try
    {
        _logger.LogInformation("استرجاع بحوث التراك {Track}", track);

        return await _dbSet
            .Where(r => r.Track == track && !r.IsDeleted)
            .Include(r => r.SubmittedBy)
            .Include(r => r.Authors.OrderBy(a => a.Order))
            .OrderByDescending(r => r.SubmissionDate)
            .ToListAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "خطأ في استرجاع بحوث التراك {Track}", track);
        throw new InvalidOperationException($"خطأ في استرجاع بحوث التراك: {ex.Message}", ex);
    }
}

public async Task<IEnumerable<Research>> GetByTrackWithDetailsAsync(ResearchTrack track)
{
    try
    {
        return await _dbSet
            .Where(r => r.Track == track && !r.IsDeleted)
            .Include(r => r.SubmittedBy)
            .Include(r => r.Authors.OrderBy(a => a.Order))
            .Include(r => r.Files.Where(f => f.IsActive))
            .Include(r => r.Reviews)
            .OrderByDescending(r => r.SubmissionDate)
            .ToListAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "خطأ في استرجاع بحوث التراك {Track} مع التفاصيل", track);
        throw new InvalidOperationException($"خطأ في استرجاع بحوث التراك مع التفاصيل: {ex.Message}", ex);
    }
}

#endregion

#region Get by Status

public async Task<IEnumerable<Research>> GetByStatusAsync(ResearchStatus status)
{
    try
    {
        _logger.LogInformation("استرجاع البحوث بالحالة {Status}", status);

        return await _dbSet
            .Where(r => r.Status == status && !r.IsDeleted)
            .Include(r => r.SubmittedBy)
            .Include(r => r.Authors.OrderBy(a => a.Order))
            .OrderByDescending(r => r.SubmissionDate)
            .ToListAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "خطأ في استرجاع البحوث بالحالة {Status}", status);
        throw new InvalidOperationException($"خطأ في استرجاع البحوث بالحالة: {ex.Message}", ex);
    }
}

public async Task<IEnumerable<Research>> GetByStatusWithDetailsAsync(ResearchStatus status)
{
    try
    {
        return await _dbSet
            .Where(r => r.Status == status && !r.IsDeleted)
            .Include(r => r.SubmittedBy)
            .Include(r => r.Authors.OrderBy(a => a.Order))
            .Include(r => r.Files.Where(f => f.IsActive))
            .Include(r => r.Reviews)
            .OrderByDescending(r => r.SubmissionDate)
            .ToListAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "خطأ في استرجاع البحوث بالحالة {Status} مع التفاصيل", status);
        throw new InvalidOperationException($"خطأ في استرجاع البحوث بالحالة مع التفاصيل: {ex.Message}", ex);
    }
}

#endregion

#region Management Methods

public async Task<IEnumerable<Research>> GetByTrackManagerAsync(int trackManagerId)
{
    try
    {
        var trackManager = await _context.TrackManagers
            .FirstOrDefaultAsync(tm => tm.Id == trackManagerId && tm.IsActive);

        if (trackManager == null)
            return new List<Research>();

        return await _dbSet
            .Where(r => r.Track == trackManager.Track && !r.IsDeleted)
            .Include(r => r.SubmittedBy)
            .Include(r => r.Authors.OrderBy(a => a.Order))
            .Include(r => r.Reviews)
            .OrderByDescending(r => r.SubmissionDate)
            .ToListAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "خطأ في استرجاع بحوث مدير التراك {TrackManagerId}", trackManagerId);
        throw new InvalidOperationException($"خطأ في استرجاع بحوث مدير التراك: {ex.Message}", ex);
    }
}

public async Task<IEnumerable<Research>> GetForReviewerAsync(string reviewerId)
{
    try
    {
        if (string.IsNullOrWhiteSpace(reviewerId))
            throw new ArgumentException("معرف المراجع مطلوب", nameof(reviewerId));

        return await _dbSet
            .Where(r => r.Reviews.Any(rv => rv.ReviewerId == reviewerId) && !r.IsDeleted)
            .Include(r => r.SubmittedBy)
            .Include(r => r.Authors.OrderBy(a => a.Order))
            .Include(r => r.Files.Where(f => f.IsActive))
            .Include(r => r.Reviews.Where(rv => rv.ReviewerId == reviewerId))
            .OrderByDescending(r => r.SubmissionDate)
            .ToListAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "خطأ في استرجاع بحوث المراجع {ReviewerId}", reviewerId);
        throw new InvalidOperationException($"خطأ في استرجاع بحوث المراجع: {ex.Message}", ex);
    }
}

#endregion

#region Date Range and Recent

public async Task<IEnumerable<Research>> GetBySubmissionDateRangeAsync(DateTime fromDate, DateTime toDate)
{
    try
    {
        return await _dbSet
            .Where(r => r.SubmissionDate.Date >= fromDate.Date &&
                       r.SubmissionDate.Date <= toDate.Date &&
                       !r.IsDeleted)
            .Include(r => r.SubmittedBy)
            .Include(r => r.Authors.OrderBy(a => a.Order))
            .OrderByDescending(r => r.SubmissionDate)
            .ToListAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "خطأ في استرجاع البحوث بالتاريخ من {FromDate} إلى {ToDate}", fromDate, toDate);
        throw new InvalidOperationException($"خطأ في استرجاع البحوث بالتاريخ: {ex.Message}", ex);
    }
}

public async Task<IEnumerable<Research>> GetRecentSubmissionsAsync(int count = 10)
{
    try
    {
        if (count <= 0) count = 10;

        return await _dbSet
            .Where(r => !r.IsDeleted)
            .Include(r => r.SubmittedBy)
            .Include(r => r.Authors.OrderBy(a => a.Order))
            .OrderByDescending(r => r.SubmissionDate)
            .Take(count)
            .ToListAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "خطأ في استرجاع البحوث الحديثة");
        throw new InvalidOperationException($"خطأ في استرجاع البحوث الحديثة: {ex.Message}", ex);
    }
}

public async Task<(IEnumerable<Research> researches, int totalCount)> GetPagedAsync(
    Dictionary<string, object> searchCriteria,
    int page,
    int pageSize,
    string sortBy,
    bool sortDescending)
{
    try
    {
        _logger.LogInformation("استرجاع البحوث مع التصفح - الصفحة: {Page}, الحجم: {PageSize}", page, pageSize);

        var query = _dbSet
            .Where(r => !r.IsDeleted)
            .Include(r => r.SubmittedBy)
            .Include(r => r.Authors.OrderBy(a => a.Order))
            .Include(r => r.Reviews)
                .ThenInclude(rv => rv.Reviewer)
            .Include(r => r.StatusHistory)
            .Include(r => r.AssignedTrackManager)
                .ThenInclude(tm => tm!.User)
            .AsQueryable();

        // تطبيق معايير البحث
        query = ApplySearchCriteria(query, searchCriteria);

        // حساب العدد الإجمالي
        var totalCount = await query.CountAsync();

        // تطبيق الترتيب
        query = ApplySorting(query, sortBy, sortDescending);

        // تطبيق التصفح
        var researches = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        _logger.LogInformation("تم استرجاع {Count} بحث من أصل {Total}", researches.Count, totalCount);

        return (researches, totalCount);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "خطأ في استرجاع البحوث مع التصفح");
        throw new InvalidOperationException($"خطأ في استرجاع البحوث مع التصفح: {ex.Message}", ex);
    }
}

private IQueryable<Research> ApplySearchCriteria(IQueryable<Research> query, Dictionary<string, object> searchCriteria)
{
    if (searchCriteria == null || !searchCriteria.Any())
        return query;

    foreach (var criteria in searchCriteria)
    {
        switch (criteria.Key.ToLower())
        {
            case "searchterm":
                if (criteria.Value is string searchTerm && !string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(r => 
                        r.Title.Contains(searchTerm) ||
                        r.Abstract.Contains(searchTerm) ||
                        r.Keywords.Contains(searchTerm) ||
                        r.Authors.Any(a => a.FirstName.Contains(searchTerm) || a.LastName.Contains(searchTerm)));
                }
                break;

            case "status":
                if (criteria.Value is ResearchStatus status)
                {
                    query = query.Where(r => r.Status == status);
                }
                break;

            case "track":
                if (criteria.Value is ResearchTrack track)
                {
                    query = query.Where(r => r.Track == track);
                }
                break;

            case "fromdate":
                if (criteria.Value is DateTime fromDate)
                {
                    query = query.Where(r => r.SubmissionDate >= fromDate);
                }
                break;

            case "todate":
                if (criteria.Value is DateTime toDate)
                {
                    query = query.Where(r => r.SubmissionDate <= toDate);
                }
                break;
        }
    }

    return query;
}

private IQueryable<Research> ApplySorting(IQueryable<Research> query, string sortBy, bool sortDescending)
{
    switch (sortBy?.ToLower())
    {
        case "title":
            return sortDescending ? query.OrderByDescending(r => r.Title) : query.OrderBy(r => r.Title);
        
        case "status":
            return sortDescending ? query.OrderByDescending(r => r.Status) : query.OrderBy(r => r.Status);
        
        case "track":
            return sortDescending ? query.OrderByDescending(r => r.Track) : query.OrderBy(r => r.Track);
        
        case "submittedby":
            return sortDescending ? 
                query.OrderByDescending(r => r.SubmittedBy.FirstName).ThenByDescending(r => r.SubmittedBy.LastName) : 
                query.OrderBy(r => r.SubmittedBy.FirstName).ThenBy(r => r.SubmittedBy.LastName);
        
        case "submissiondate":
        default:
            return sortDescending ? query.OrderByDescending(r => r.SubmissionDate) : query.OrderBy(r => r.SubmissionDate);
    }
}

#endregion

    }
}
