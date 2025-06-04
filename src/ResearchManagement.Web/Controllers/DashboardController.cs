using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Infrastructure.Data;
using ResearchManagement.Web.Models.ViewModels;

namespace ResearchManagement.Web.Controllers
{
    [Authorize]
    public class DashboardController : BaseController
    {
        private readonly IResearchRepository _researchRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            UserManager<User> userManager,
            IResearchRepository researchRepository,
            IReviewRepository reviewRepository,
            ApplicationDbContext context,
            ILogger<DashboardController> logger) : base(userManager)
        {
            _researchRepository = researchRepository ?? throw new ArgumentNullException(nameof(researchRepository));
            _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    _logger.LogWarning("User not authenticated, redirecting to login");
                    return RedirectToAction("Login", "Account");
                }

                _logger.LogInformation("Loading dashboard for user {UserId} with role {Role}", user.Id, user.Role);

                ViewData["UserRole"] = user.Role;
                ViewData["UserName"] = $"{user.FirstName} {user.LastName}";

                return user.Role switch
                {
                    UserRole.Researcher => await ResearcherDashboard(),
                    UserRole.Reviewer => await ReviewerDashboard(),
                    UserRole.TrackManager => await TrackManagerDashboard(),
                    UserRole.ConferenceManager or UserRole.SystemAdmin => await ConferenceManagerDashboard(),
                    _ => View("UnknownRole")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                AddErrorMessage("حدث خطأ في تحميل لوحة التحكم");
                return View("Error");
            }
        }

        private async Task<IActionResult> ResearcherDashboard()
        {
            try
            {
                var userId = GetCurrentUserId();
                var researches = await _researchRepository.GetByUserIdAsync(userId);

                var viewModel = new ResearcherDashboardViewModel
                {
                    TotalResearches = researches.Count(),
                    AcceptedResearches = researches.Count(r => r.Status == ResearchStatus.Accepted),
                    PendingResearches = researches.Count(r => r.Status == ResearchStatus.Submitted ||
                                                             r.Status == ResearchStatus.UnderReview ||
                                                             r.Status == ResearchStatus.UnderEvaluation),
                    RejectedResearches = researches.Count(r => r.Status == ResearchStatus.Rejected),
                    RecentResearches = researches.OrderByDescending(r => r.SubmissionDate).Take(5).ToList()
                };

                _logger.LogInformation("Researcher dashboard loaded successfully for user {UserId}", userId);
                return View("ResearcherDashboard", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading researcher dashboard");
                AddErrorMessage("حدث خطأ في تحميل لوحة تحكم الباحث");
                return View("Error");
            }
        }

        private async Task<IActionResult> ReviewerDashboard()
        {
            try
            {
                var userId = GetCurrentUserId();
                var reviews = await _reviewRepository.GetByReviewerIdAsync(userId);
                var pendingReviews = await _reviewRepository.GetPendingReviewsAsync(userId);

                var viewModel = new ReviewerDashboardViewModel
                {
                    TotalReviews = reviews.Count(),
                    CompletedReviews = reviews.Count(r => r.IsCompleted),
                    PendingReviews = pendingReviews.Count(),
                    OverdueReviews = pendingReviews.Count(r => r.Deadline < DateTime.UtcNow),
                    RecentReviews = reviews.OrderByDescending(r => r.AssignedDate).Take(5).ToList()
                };

                _logger.LogInformation("Reviewer dashboard loaded successfully for user {UserId}", userId);
                return View("ReviewerDashboard", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reviewer dashboard");
                AddErrorMessage("حدث خطأ في تحميل لوحة تحكم المقيم");
                return View("Error");
            }
        }

        private async Task<IActionResult> TrackManagerDashboard()
        {
            try
            {
                var userId = GetCurrentUserId();

                var trackManager = await _context.TrackManagers
                    .Include(tm => tm.TrackReviewers)
                    .ThenInclude(tr => tr.Reviewer)
                    .FirstOrDefaultAsync(tm => tm.UserId == userId && tm.IsActive);

                if (trackManager == null)
                {
                    _logger.LogWarning("TrackManager not found for user {UserId}", userId);
                    AddErrorMessage("لم يتم العثور على معلومات مدير التراك");
                    return View("Error");
                }

                var trackResearches = await _context.Researches
                    .Include(r => r.SubmittedBy)
                    .Include(r => r.Authors)
                    .Where(r => r.Track == trackManager.Track)
                    .ToListAsync();

                var trackReviews = await _context.Reviews
                    .Include(r => r.Research)
                    .Include(r => r.Reviewer)
                    .Where(r => r.Research.Track == trackManager.Track)
                    .ToListAsync();

                var viewModel = new TrackManagerDashboardViewModel
                {
                    TotalResearches = trackResearches.Count,
                    TotalReviewers = trackManager.TrackReviewers.Count(tr => tr.IsActive),
                    PendingAssignments = trackResearches.Count(r => r.Status == ResearchStatus.Submitted),
                    CompletedReviews = trackReviews.Count(r => r.IsCompleted),
                    TrackName = GetTrackDisplayName(trackManager.Track),
                    RecentResearches = trackResearches.OrderByDescending(r => r.SubmissionDate).Take(5).ToList(),
                    OverdueReviews = trackReviews.Where(r => !r.IsCompleted && r.Deadline < DateTime.UtcNow).Take(5).ToList()
                };

                _logger.LogInformation("Track manager dashboard loaded successfully for user {UserId}, track {Track}", userId, trackManager.Track);
                return View("TrackManagerDashboard", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading track manager dashboard");
                AddErrorMessage("حدث خطأ في تحميل لوحة تحكم مدير التراك");
                return View("Error");
            }
        }

        private async Task<IActionResult> ConferenceManagerDashboard()
        {
            try
            {
                var allResearches = await _context.Researches
                    .Include(r => r.SubmittedBy)
                    .Include(r => r.Authors)
                    .ToListAsync();

                var allUsers = await _context.Users
                    .Where(u => u.IsActive)
                    .OrderByDescending(u => u.CreatedAt)
                    .ToListAsync();

                var allReviews = await _context.Reviews
                    .Where(r => r.IsCompleted)
                    .ToListAsync();

                var trackStatistics = allResearches
                    .GroupBy(r => r.Track)
                    .Select(g => new TrackStatistic
                    {
                        TrackName = GetTrackDisplayName(g.Key),
                        ResearchCount = g.Count(),
                        AcceptedCount = g.Count(r => r.Status == ResearchStatus.Accepted),
                        RejectedCount = g.Count(r => r.Status == ResearchStatus.Rejected),
                        PendingCount = g.Count(r => r.Status == ResearchStatus.UnderReview ||
                                              r.Status == ResearchStatus.UnderEvaluation)
                    })
                    .OrderByDescending(x => x.ResearchCount)
                    .ToList();

                var averageReviewTime = allReviews.Any() ?
                    allReviews.Where(r => r.CompletedDate.HasValue)
                             .Average(r => (r.CompletedDate!.Value - r.AssignedDate).TotalDays) : 0;

                var viewModel = new ConferenceManagerDashboardViewModel
                {
                    TotalResearches = allResearches.Count,
                    AcceptedResearches = allResearches.Count(r => r.Status == ResearchStatus.Accepted),
                    PendingResearches = allResearches.Count(r => r.Status == ResearchStatus.UnderReview ||
                                                                r.Status == ResearchStatus.UnderEvaluation ||
                                                                r.Status == ResearchStatus.AssignedForReview),
                    RejectedResearches = allResearches.Count(r => r.Status == ResearchStatus.Rejected),
                    SubmittedResearches = allResearches.Count(r => r.Status == ResearchStatus.Submitted),
                    TotalUsers = allUsers.Count,
                    TotalReviewers = allUsers.Count(u => u.Role == UserRole.Reviewer),
                    CompletedReviews = allReviews.Count,
                    AverageReviewTime = Math.Round(averageReviewTime, 1),
                    RecentSubmissions = allResearches.OrderByDescending(r => r.SubmissionDate).Take(10).ToList(),
                    RecentUsers = allUsers.Take(5).ToList(),
                    TrackStatistics = trackStatistics
                };

                _logger.LogInformation("Conference manager dashboard loaded successfully");
                return View("ConferenceManagerDashboard", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading conference manager dashboard");
                AddErrorMessage("حدث خطأ في تحميل لوحة تحكم مدير المؤتمر");
                return View("Error");
            }
        }
        // ============================================
        // دوال مساعدة لعرض الأسماء بالعربية
        // ============================================

        /// <summary>
        /// دالة لعرض أسماء التخصصات بالعربية
        /// </summary>
        private string GetTrackDisplayName(ResearchTrack track)
        {
            return track switch
            {
                ResearchTrack.InformationTechnology => "تقنية المعلومات",
                ResearchTrack.InformationSecurity => "أمن المعلومات",
                ResearchTrack.ArtificialIntelligence => "الذكاء الاصطناعي",
                ResearchTrack.DataScience => "علوم البيانات",
                ResearchTrack.SoftwareEngineering => "هندسة البرمجيات",
                ResearchTrack.NetworkingAndCommunications => "الشبكات والاتصالات",
                ResearchTrack.CloudComputing => "الحوسبة السحابية",
                ResearchTrack.InternetOfThings => "إنترنت الأشياء",
                ResearchTrack.ARAndVR => "الواقع المعزز والافتراضي",
                ResearchTrack.Blockchain => "البلوك تشين",
                ResearchTrack.MachineLearning => "التعلم الآلي",
                ResearchTrack.NaturalLanguageProcessing => "معالجة اللغات الطبيعية",
                ResearchTrack.HighPerformanceComputing => "الحوسبة عالية الأداء",
                ResearchTrack.MobileAppDevelopment => "تطوير التطبيقات المحمولة",
                ResearchTrack.DatabaseSystems => "قواعد البيانات",
                _ => track.ToString()
            };
        }

        /// <summary>
        /// دالة لعرض أسماء حالات البحوث بالعربية
        /// </summary>
        private string GetStatusDisplayName(ResearchStatus status)
        {
            return status switch
            {
                ResearchStatus.Submitted => "مقدم",
                ResearchStatus.UnderInitialReview => "قيد المراجعة الأولية",
                ResearchStatus.AssignedForReview => "موزع للتقييم",
                ResearchStatus.UnderReview => "قيد التقييم",
                ResearchStatus.UnderEvaluation => "تحت المراجعة",
                ResearchStatus.RequiresMinorRevisions => "يتطلب تعديلات طفيفة",
                ResearchStatus.RequiresMajorRevisions => "يتطلب تعديلات جوهرية",
                ResearchStatus.RevisionsSubmitted => "تعديلات مقدمة",
                ResearchStatus.RevisionsUnderReview => "مراجعة التعديلات",
                ResearchStatus.Accepted => "مقبول",
                ResearchStatus.Rejected => "مرفوض",
                ResearchStatus.Withdrawn => "منسحب",
                ResearchStatus.AwaitingFourthReviewer => "بانتظار المقيم الرابع",
                _ => status.ToString()
            };
        }

        /// <summary>
        /// دالة لعرض أسماء الأدوار بالعربية
        /// </summary>
        private string GetRoleDisplayName(UserRole role)
        {
            return role switch
            {
                UserRole.Researcher => "باحث",
                UserRole.Reviewer => "مقيم",
                UserRole.TrackManager => "مدير تراك",
                UserRole.ConferenceManager => "مدير المؤتمر",
                UserRole.SystemAdmin => "مدير النظام",
                _ => role.ToString()
            };
        }

        /// <summary>
        /// دالة لعرض أسماء قرارات المراجعة بالعربية
        /// </summary>
        private string GetDecisionDisplayName(ReviewDecision decision)
        {
            return decision switch
            {
                ReviewDecision.NotReviewed => "لم يتم المراجعة بعد",
                ReviewDecision.AcceptAsIs => "قبول فوري",
                ReviewDecision.AcceptWithMinorRevisions => "قبول مع تعديلات طفيفة",
                ReviewDecision.MajorRevisionsRequired => "تعديلات جوهرية مطلوبة",
                ReviewDecision.Reject => "رفض",
                ReviewDecision.NotSuitableForConference => "غير مناسب للمؤتمر",
                _ => decision.ToString()
            };
        }

        // ============================================
        // API Actions للحصول على إحصائيات سريعة
        // ============================================

        [HttpGet]
        public async Task<IActionResult> GetQuickStats()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return Json(new { success = false, message = "غير مصرح" });

                var stats = user.Role switch
                {
                    UserRole.Researcher => await GetResearcherQuickStats(user.Id),
                    UserRole.Reviewer => await GetReviewerQuickStats(user.Id),
                    UserRole.TrackManager => await GetTrackManagerQuickStats(user.Id),
                    UserRole.ConferenceManager or UserRole.SystemAdmin => await GetConferenceManagerQuickStats(),
                    _ => new { }
                };

                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quick stats");
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<object> GetResearcherQuickStats(string userId)
        {
            var researches = await _researchRepository.GetByUserIdAsync(userId);
            return new
            {
                total = researches.Count(),
                accepted = researches.Count(r => r.Status == ResearchStatus.Accepted),
                pending = researches.Count(r => r.Status == ResearchStatus.UnderReview),
                rejected = researches.Count(r => r.Status == ResearchStatus.Rejected)
            };
        }

        private async Task<object> GetReviewerQuickStats(string userId)
        {
            var reviews = await _reviewRepository.GetByReviewerIdAsync(userId);
            var pending = await _reviewRepository.GetPendingReviewsAsync(userId);

            return new
            {
                total = reviews.Count(),
                completed = reviews.Count(r => r.IsCompleted),
                pending = pending.Count(),
                overdue = pending.Count(r => r.Deadline < DateTime.UtcNow)
            };
        }

        private async Task<object> GetTrackManagerQuickStats(string userId)
        {
            var trackManager = await _context.TrackManagers
                .FirstOrDefaultAsync(tm => tm.UserId == userId && tm.IsActive);

            if (trackManager == null)
                return new { };

            var trackResearches = await _context.Researches
                .Where(r => r.Track == trackManager.Track)
                .CountAsync();

            return new
            {
                totalResearches = trackResearches,
                pendingAssignments = await _context.Researches
                    .CountAsync(r => r.Track == trackManager.Track && r.Status == ResearchStatus.Submitted)
            };
        }

        private async Task<object> GetConferenceManagerQuickStats()
        {
            var totalResearches = await _context.Researches.CountAsync();
            var totalUsers = await _context.Users.CountAsync(u => u.IsActive);

            return new
            {
                totalResearches,
                totalUsers,
                acceptedResearches = await _context.Researches.CountAsync(r => r.Status == ResearchStatus.Accepted),
                pendingResearches = await _context.Researches.CountAsync(r =>
                    r.Status == ResearchStatus.UnderReview || r.Status == ResearchStatus.UnderEvaluation)
            };
        }

        // ============================================
        // تصدير الإحصائيات للتقارير
        // ============================================

        [HttpGet]
        [Authorize(Roles = "ConferenceManager,SystemAdmin")]
        public async Task<IActionResult> ExportStatistics()
        {
            try
            {
                var allResearches = await _context.Researches.ToListAsync();
                var allUsers = await _context.Users.ToListAsync();
                var allReviews = await _context.Reviews.ToListAsync();

                var exportData = new
                {
                    GeneratedAt = DateTime.UtcNow,
                    TotalResearches = allResearches.Count,
                    TotalUsers = allUsers.Count,
                    TotalReviews = allReviews.Count,
                    ResearchesByStatus = allResearches.GroupBy(r => r.Status)
                        .ToDictionary(g => GetStatusDisplayName(g.Key), g => g.Count()),
                    ResearchesByTrack = allResearches.GroupBy(r => r.Track)
                        .ToDictionary(g => GetTrackDisplayName(g.Key), g => g.Count()),
                    UsersByRole = allUsers.GroupBy(u => u.Role)
                        .ToDictionary(g => GetRoleDisplayName(g.Key), g => g.Count())
                };

                return Json(exportData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting statistics");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================================
        // تحديث إحصائيات لوحة التحكم
        // ============================================

        [HttpPost]
        public async Task<IActionResult> RefreshDashboard()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return Json(new { success = false, message = "غير مصرح" });

                // إعادة تحديث البيانات حسب نوع المستخدم
                var result = user.Role switch
                {
                    UserRole.Researcher => await GetResearcherQuickStats(user.Id),
                    UserRole.Reviewer => await GetReviewerQuickStats(user.Id),
                    UserRole.TrackManager => await GetTrackManagerQuickStats(user.Id),
                    UserRole.ConferenceManager or UserRole.SystemAdmin => await GetConferenceManagerQuickStats(),
                    _ => new { }
                };

                return Json(new { success = true, data = result, refreshTime = DateTime.Now });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing dashboard");
                return Json(new { success = false, message = "حدث خطأ في تحديث البيانات" });
            }
        }
    }
}