using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Infrastructure.Data;
using ResearchManagement.Web.Models.ViewModels.TrackManager;
//using ResearchManagement.Web.Models.ViewModels.TrackManagement;
using ResearchManagement.Web.Models.ViewModels;

namespace ResearchManagement.Web.Controllers
{
    [Authorize(Roles = "TrackManager,SystemAdmin")]
    public class TrackManagerController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IResearchRepository _researchRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<TrackManagerController> _logger;

        public TrackManagerController(
            UserManager<User> userManager,
            ApplicationDbContext context,
            IResearchRepository researchRepository,
            IReviewRepository reviewRepository,
            IEmailService emailService,
            ILogger<TrackManagerController> logger) : base(userManager)
        {
            _context = context;
            _researchRepository = researchRepository;
            _reviewRepository = reviewRepository;
            _emailService = emailService;
            _logger = logger;
        }

        // GET: TrackManager
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var trackManager = await GetTrackManagerAsync(user.Id);
                if (trackManager == null)
                {
                    AddErrorMessage("لم يتم العثور على معلومات مدير المسار");
                    return RedirectToAction("Index", "Dashboard");
                }

                var trackResearches = await _context.Researches
                    .Include(r => r.SubmittedBy)
                    .Include(r => r.Authors)
                    .Include(r => r.Reviews)
                        .ThenInclude(rv => rv.Reviewer)
                    .Where(r => r.Track == trackManager.Track && !r.IsDeleted)
                    .OrderByDescending(r => r.SubmissionDate)
                    .ToListAsync();

                var statistics = CalculateTrackStatistics(trackResearches);

                var viewModel = new TrackManagerDashboardViewModel
                {
                    TrackName = GetTrackDisplayName(trackManager.Track),
                    Track = trackManager.Track,
                    Statistics = statistics,
                    RecentResearches = trackResearches.Take(5).ToList(),
                    PendingResearches = trackResearches
                        .Where(r => r.Status == ResearchStatus.Submitted)
                        .ToList(),
                    ResearchesNeedingReviewers = trackResearches
                        .Where(r => r.Status == ResearchStatus.UnderInitialReview &&
                                   r.Reviews.Count < 3)
                        .ToList(),
                    OverdueReviews = await GetOverdueReviews(trackManager.Track)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading track manager dashboard for user {UserId}", GetCurrentUserId());
                AddErrorMessage("حدث خطأ في تحميل لوحة تحكم مدير المسار");
                return RedirectToAction("Index", "Dashboard");
            }
        }

        // GET: TrackManager/Researches
        public async Task<IActionResult> Researches(
            string? searchTerm,
            ResearchStatus? status,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var trackManager = await GetTrackManagerAsync(user.Id);

                if (trackManager == null)
                {
                    AddErrorMessage("غير مصرح لك بالوصول لهذه الصفحة");
                    return RedirectToAction("Index", "Dashboard");
                }

                var query = _context.Researches
                    .Include(r => r.SubmittedBy)
                    .Include(r => r.Authors)
                    .Include(r => r.Reviews)
                    .Include(r => r.Files)
                    .Where(r => r.Track == trackManager.Track && !r.IsDeleted);

                // التصفية
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(r => r.Title.Contains(searchTerm) ||
                                           r.AbstractAr.Contains(searchTerm));
                }

                if (status.HasValue)
                {
                    query = query.Where(r => r.Status == status.Value);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(r => r.SubmissionDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(r => r.SubmissionDate <= toDate.Value);
                }

                var totalCount = await query.CountAsync();
                var researches = await query
                    .OrderByDescending(r => r.SubmissionDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var viewModel = new TrackResearchesViewModel
                {
                    Researches = researches,
                    TrackName = GetTrackDisplayName(trackManager.Track),
                    Track = trackManager.Track,
                    Filter = new ResearchFilterDto
                    {
                        SearchTerm = searchTerm,
                        Status = status,
                        FromDate = fromDate,
                        ToDate = toDate
                    },
                    Pagination = new PaginationDto
                    {
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading track researches for user {UserId}", GetCurrentUserId());
                AddErrorMessage("حدث خطأ في تحميل بحوث المسار");
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: TrackManager/AssignReviewers/5
        public async Task<IActionResult> AssignReviewers(int researchId)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var trackManager = await GetTrackManagerAsync(user.Id);

                if (trackManager == null)
                {
                    AddErrorMessage("غير مصرح لك بهذا الإجراء");
                    return RedirectToAction(nameof(Index));
                }

                var research = await _context.Researches
                    .Include(r => r.Reviews)
                        .ThenInclude(rv => rv.Reviewer)
                    .Include(r => r.Authors)
                    .FirstOrDefaultAsync(r => r.Id == researchId && r.Track == trackManager.Track);

                if (research == null)
                {
                    AddErrorMessage("البحث غير موجود أو لا ينتمي لمسارك");
                    return RedirectToAction(nameof(Researches));
                }

                // جلب المراجعين المتاحين للمسار
                var availableReviewers = await _context.TrackReviewers
                    .Include(tr => tr.Reviewer)
                    .Where(tr => tr.Track == trackManager.Track && tr.IsActive &&
                                tr.Reviewer.IsActive)
                    .Select(tr => tr.Reviewer)
                    .ToListAsync();

                // استبعاد المراجعين المعينين مسبقاً
                var assignedReviewerIds = research.Reviews.Select(r => r.ReviewerId).ToList();
                availableReviewers = availableReviewers
                    .Where(r => !assignedReviewerIds.Contains(r.Id))
                    .ToList();

                var viewModel = new AssignReviewersViewModel
                {
                    Research = research,
                    AvailableReviewers = availableReviewers,
                    CurrentReviews = research.Reviews.ToList(),
                    TrackName = GetTrackDisplayName(trackManager.Track)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading assign reviewers page for research {ResearchId}", researchId);
                AddErrorMessage("حدث خطأ في تحميل صفحة تعيين المراجعين");
                return RedirectToAction(nameof(Researches));
            }
        }

        // POST: TrackManager/AssignReviewer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignReviewer(int researchId, string reviewerId, DateTime? deadline)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var trackManager = await GetTrackManagerAsync(user.Id);

                if (trackManager == null)
                {
                    return Json(new { success = false, message = "غير مصرح لك بهذا الإجراء" });
                }

                var research = await _context.Researches
                    .Include(r => r.Reviews)
                    .FirstOrDefaultAsync(r => r.Id == researchId && r.Track == trackManager.Track);

                if (research == null)
                {
                    return Json(new { success = false, message = "البحث غير موجود" });
                }

                // التحقق من أن المراجع غير معين مسبقاً
                var existingReview = research.Reviews.FirstOrDefault(r => r.ReviewerId == reviewerId);
                if (existingReview != null)
                {
                    return Json(new { success = false, message = "المراجع معين مسبقاً لهذا البحث" });
                }

                // إنشاء مراجعة جديدة
                var review = new Review
                {
                    ResearchId = researchId,
                    ReviewerId = reviewerId,
                    AssignedDate = DateTime.UtcNow,
                    Deadline = deadline ?? DateTime.UtcNow.AddDays(14),
                    Decision = ReviewDecision.NotReviewed,
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = user.Id
                };

                _context.Reviews.Add(review);

                // تحديث حالة البحث
                if (research.Status == ResearchStatus.Submitted)
                {
                    research.Status = ResearchStatus.AssignedForReview;
                    research.UpdatedAt = DateTime.UtcNow;
                    research.UpdatedBy = user.Id;
                }

                await _context.SaveChangesAsync();

                // إرسال إشعار للمراجع
                try
                {
                    await _emailService.SendReviewAssignmentNotificationAsync(review.Id);
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "فشل في إرسال إشعار تعيين المراجعة");
                }

                _logger.LogInformation("تم تعيين المراجع {ReviewerId} للبحث {ResearchId} بواسطة {UserId}",
                    reviewerId, researchId, user.Id);

                return Json(new { success = true, message = "تم تعيين المراجع بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning reviewer {ReviewerId} to research {ResearchId}",
                    reviewerId, researchId);
                return Json(new { success = false, message = "حدث خطأ في تعيين المراجع" });
            }
        }

        // POST: TrackManager/RemoveReviewer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveReviewer(int reviewId)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var trackManager = await GetTrackManagerAsync(user.Id);

                if (trackManager == null)
                {
                    return Json(new { success = false, message = "غير مصرح لك بهذا الإجراء" });
                }

                var review = await _context.Reviews
                    .Include(r => r.Research)
                    .FirstOrDefaultAsync(r => r.Id == reviewId &&
                                            r.Research.Track == trackManager.Track);

                if (review == null)
                {
                    return Json(new { success = false, message = "المراجعة غير موجودة" });
                }

                if (review.IsCompleted)
                {
                    return Json(new { success = false, message = "لا يمكن إزالة مراجع أكمل المراجعة" });
                }

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                _logger.LogInformation("تم إزالة المراجع من البحث {ResearchId} بواسطة {UserId}",
                    review.ResearchId, user.Id);

                return Json(new { success = true, message = "تم إزالة المراجع بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing reviewer {ReviewId}", reviewId);
                return Json(new { success = false, message = "حدث خطأ في إزالة المراجع" });
            }
        }

        // GET: TrackManager/ReviewerManagement
        public async Task<IActionResult> ReviewerManagement()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var trackManager = await GetTrackManagerAsync(user.Id);

                if (trackManager == null)
                {
                    AddErrorMessage("غير مصرح لك بالوصول لهذه الصفحة");
                    return RedirectToAction(nameof(Index));
                }

                var trackReviewers = await _context.TrackReviewers
                    .Include(tr => tr.Reviewer)
                    .Where(tr => tr.Track == trackManager.Track)
                    .ToListAsync();

                var reviewerStatistics = new List<ReviewerStatisticsDto>();

                foreach (var trackReviewer in trackReviewers)
                {
                    var reviews = await _context.Reviews
                        .Include(r => r.Research)
                        .Where(r => r.ReviewerId == trackReviewer.ReviewerId &&
                                   r.Research.Track == trackManager.Track)
                        .ToListAsync();

                    var completedReviews = reviews.Where(r => r.IsCompleted).ToList();

                    var statistics = new ReviewerStatisticsDto
                    {
                        ReviewerId = trackReviewer.ReviewerId,
                        ReviewerName = $"{trackReviewer.Reviewer.FirstName} {trackReviewer.Reviewer.LastName}",
                        TotalAssigned = reviews.Count,
                        CompletedReviews = completedReviews.Count,
                        PendingReviews = reviews.Count(r => !r.IsCompleted),
                        OverdueReviews = reviews.Count(r => !r.IsCompleted && r.Deadline < DateTime.UtcNow),
                        AverageReviewTime = completedReviews.Any() && completedReviews.All(r => r.CompletedDate.HasValue)
                            ? completedReviews.Average(r => (r.CompletedDate!.Value - r.AssignedDate).TotalDays)
                            : 0,
                        AverageScore = completedReviews.Any()
                            ? (double)completedReviews.Average(r => r.OverallScore)
                            : 0,
                        IsActive = trackReviewer.IsActive
                    };

                    reviewerStatistics.Add(statistics);
                }

                var viewModel = new ReviewerManagementViewModel
                {
                    TrackName = GetTrackDisplayName(trackManager.Track),
                    Track = trackManager.Track,
                    ReviewerStatistics = reviewerStatistics
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reviewer management for user {UserId}", GetCurrentUserId());
                AddErrorMessage("حدث خطأ في تحميل إدارة المراجعين");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: TrackManager/UpdateResearchStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateResearchStatus(int researchId, ResearchStatus newStatus, string? notes)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var trackManager = await GetTrackManagerAsync(user.Id);

                if (trackManager == null)
                {
                    return Json(new { success = false, message = "غير مصرح لك بهذا الإجراء" });
                }

                var research = await _context.Researches
                    .Include(r => r.Reviews)
                    .FirstOrDefaultAsync(r => r.Id == researchId && r.Track == trackManager.Track);

                if (research == null)
                {
                    return Json(new { success = false, message = "البحث غير موجود" });
                }

                var oldStatus = research.Status;
                research.Status = newStatus;
                research.UpdatedAt = DateTime.UtcNow;
                research.UpdatedBy = user.Id;

                // إضافة سجل في تاريخ الحالات
                var statusHistory = new ResearchStatusHistory
                {
                    ResearchId = researchId,
                    FromStatus = oldStatus,
                    ToStatus = newStatus,
                    Notes = notes,
                    ChangedById = user.Id,
                    ChangedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = user.Id
                };

                _context.ResearchStatusHistories.Add(statusHistory);
                await _context.SaveChangesAsync();

                // إرسال إشعار للباحث
                try
                {
                    await _emailService.SendResearchStatusUpdateAsync(researchId, oldStatus, newStatus);
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "فشل في إرسال إشعار تحديث الحالة");
                }

                _logger.LogInformation("تم تحديث حالة البحث {ResearchId} من {OldStatus} إلى {NewStatus} بواسطة {UserId}",
                    researchId, oldStatus, newStatus, user.Id);

                return Json(new { success = true, message = "تم تحديث حالة البحث بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating research status for {ResearchId}", researchId);
                return Json(new { success = false, message = "حدث خطأ في تحديث حالة البحث" });
            }
        }

        // GET: TrackManager/Reports
        public async Task<IActionResult> Reports()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var trackManager = await GetTrackManagerAsync(user.Id);

                if (trackManager == null)
                {
                    AddErrorMessage("غير مصرح لك بالوصول لهذه الصفحة");
                    return RedirectToAction(nameof(Index));
                }

                var researches = await _context.Researches
                    .Include(r => r.Reviews)
                    .Include(r => r.SubmittedBy)
                    .Where(r => r.Track == trackManager.Track && !r.IsDeleted)
                    .ToListAsync();

                var reviews = await _context.Reviews
                    .Include(r => r.Research)
                    .Include(r => r.Reviewer)
                    .Where(r => r.Research.Track == trackManager.Track)
                    .ToListAsync();

                var report = new TrackReportDto
                {
                    TrackName = GetTrackDisplayName(trackManager.Track),
                    ReportGeneratedAt = DateTime.UtcNow,
                    TotalResearches = researches.Count,
                    ResearchesByStatus = researches.GroupBy(r => r.Status)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    ResearchesByMonth = researches.GroupBy(r => new { r.SubmissionDate.Year, r.SubmissionDate.Month })
                        .ToDictionary(g => $"{g.Key.Year}-{g.Key.Month:00}", g => g.Count()),
                    AverageReviewTime = reviews.Where(r => r.IsCompleted && r.CompletedDate.HasValue)
                        .DefaultIfEmpty()
                        .Average(r => r?.CompletedDate != null ?
                            (r.CompletedDate.Value - r.AssignedDate).TotalDays : 0),
                    ReviewerPerformance = reviews.GroupBy(r => r.ReviewerId)
                        .Select(g => new ReviewerPerformanceDto
                        {
                            ReviewerId = g.Key,
                            ReviewerName = g.First().Reviewer.FirstName + " " + g.First().Reviewer.LastName,
                            TotalReviews = g.Count(),
                            CompletedReviews = g.Count(r => r.IsCompleted),
                            AverageScore = g.Where(r => r.IsCompleted).DefaultIfEmpty()
                                .Average(r => r?.OverallScore ?? 0)
                        }).ToList()
                };

                return View(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating track reports for user {UserId}", GetCurrentUserId());
                AddErrorMessage("حدث خطأ في إنشاء التقارير");
                return RedirectToAction(nameof(Index));
            }
        }

        #region Helper Methods

        private async Task<TrackManager?> GetTrackManagerAsync(string userId)
        {
            return await _context.TrackManagers
                .FirstOrDefaultAsync(tm => tm.UserId == userId && tm.IsActive);
        }

        private static TrackStatisticsDto CalculateTrackStatistics(List<Research> researches)
        {
            return new TrackStatisticsDto
            {
                TotalResearches = researches.Count,
                SubmittedResearches = researches.Count(r => r.Status == ResearchStatus.Submitted),
                UnderReviewResearches = researches.Count(r => r.Status == ResearchStatus.UnderReview ||
                                                              r.Status == ResearchStatus.AssignedForReview),
                AcceptedResearches = researches.Count(r => r.Status == ResearchStatus.Accepted),
                RejectedResearches = researches.Count(r => r.Status == ResearchStatus.Rejected),
                RequiringRevisionResearches = researches.Count(r => r.Status == ResearchStatus.RequiresMinorRevisions ||
                                                                   r.Status == ResearchStatus.RequiresMajorRevisions),
                AverageProcessingTime = researches.Where(r => r.DecisionDate.HasValue)
                    .DefaultIfEmpty()
                    .Average(r => r?.DecisionDate != null ?
                        (r.DecisionDate.Value - r.SubmissionDate).TotalDays : 0)
            };
        }

        private async Task<List<Review>> GetOverdueReviews(ResearchTrack track)
        {
            return await _context.Reviews
                .Include(r => r.Research)
                .Include(r => r.Reviewer)
                .Where(r => r.Research.Track == track &&
                           !r.IsCompleted &&
                           r.Deadline < DateTime.UtcNow)
                .OrderBy(r => r.Deadline)
                .ToListAsync();
        }

        private static string GetTrackDisplayName(ResearchTrack track) => track switch
        {
            ResearchTrack.InformationTechnology => "تقنية المعلومات",
            ResearchTrack.InformationSecurity => "أمن المعلومات",
            ResearchTrack.ArtificialIntelligence => "الذكاء الاصطناعي",
            ResearchTrack.DataScience => "علوم البيانات",
            ResearchTrack.SoftwareEngineering => "هندسة البرمجيات",
            ResearchTrack.NetworkingAndCommunications => "الشبكات والاتصالات",
            _ => track.ToString()
        };

        #endregion
    }
}