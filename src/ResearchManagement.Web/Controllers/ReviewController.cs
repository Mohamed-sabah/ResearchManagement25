
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using AutoMapper;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Commands.Review;
using ResearchManagement.Application.Queries.Review;
using ResearchManagement.Web.Models.ViewModels.Review;
using ResearchManagement.Application.Queries.Research;

namespace ResearchManagement.Web.Controllers
{
    [Authorize(Roles = "Reviewer,TrackManager,SystemAdmin")]
    public class ReviewController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(
            UserManager<User> userManager,
            IMediator mediator,
            IMapper mapper,
            ILogger<ReviewController> logger) : base(userManager)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: Review
        public async Task<IActionResult> Index(
            string? searchTerm,
            string? status,
            string? track,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var query = new GetReviewListQuery
                {
                    UserId = user.Id,
                    UserRole = user.Role,
                    SearchTerm = searchTerm,
                    Status = status,
                    Track = track,
                    FromDate = fromDate,
                    ToDate = toDate,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _mediator.Send(query);
                var statistics = await GetReviewStatistics(user.Id, user.Role);

                var viewModel = new ReviewListViewModel
                {
                    Reviews = result,
                    Statistics = statistics,
                    Filter = new ReviewFilterViewModel
                    {
                        SearchTerm = searchTerm,
                        Status = status,
                        Track = track,
                        FromDate = fromDate,
                        ToDate = toDate,
                        Page = page,
                        PageSize = pageSize
                    },
                    TrackOptions = GetTrackOptions(),
                    CurrentUserId = user.Id,
                    CurrentUserRole = user.Role,
                    CanCreateReview = user.Role == UserRole.Reviewer || user.Role == UserRole.SystemAdmin
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reviews for user {UserId}", GetCurrentUserId());
                TempData["ErrorMessage"] = "حدث خطأ في تحميل المراجعات";
                return View(new ReviewListViewModel());
            }
        }

        // GET: Review/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var query = new GetReviewByIdQuery(id, user.Id);
                var review = await _mediator.Send(query);

                if (review == null)
                {
                    TempData["ErrorMessage"] = "المراجعة غير موجودة أو ليس لديك صلاحية للوصول إليها";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new ReviewDetailsViewModel
                {
                    Review = review,
                    CurrentUserId = user.Id,
                    CurrentUserRole = user.Role,
                    CanEdit = CanEditReview(review, user),
                    CanDelete = CanDeleteReview(review, user),
                    IsReviewer = review.ReviewerId == user.Id
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading review details for ID: {ReviewId}", id);
                TempData["ErrorMessage"] = "حدث خطأ في تحميل تفاصيل المراجعة";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Review/Create
        [Authorize(Roles = "Reviewer,SystemAdmin")]
        public async Task<IActionResult> Create(int researchId)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return RedirectToAction("Login", "Account");

                // جلب معلومات البحث
                var researchQuery = new GetResearchByIdQuery(researchId, user.Id);
                var research = await _mediator.Send(researchQuery);

                if (research == null)
                {
                    TempData["ErrorMessage"] = "البحث غير موجود أو ليس لديك صلاحية للوصول إليه";
                    return RedirectToAction(nameof(Index));
                }

                // التحقق من وجود مراجعة سابقة
                var existingReviewQuery = new GetReviewByResearchAndReviewerQuery(researchId, user.Id);
                var existingReview = await _mediator.Send(existingReviewQuery);

                if (existingReview != null)
                {
                    if (existingReview.IsCompleted)
                    {
                        TempData["WarningMessage"] = "لقد قمت بإكمال مراجعة هذا البحث مسبقاً";
                        return RedirectToAction("Details", new { id = existingReview.Id });
                    }
                    else
                    {
                        return RedirectToAction("Edit", new { id = existingReview.Id });
                    }
                }

                var viewModel = new CreateReviewViewModel
                {
                    ResearchId = researchId,
                    ReviewerId = user.Id,
                    Research = _mapper.Map<ResearchSummaryDto>(research),
                    Deadline = DateTime.UtcNow.AddDays(14) // موعد نهائي افتراضي 14 يوم
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create review page for research {ResearchId}", researchId);
                TempData["ErrorMessage"] = "حدث خطأ في تحميل صفحة المراجعة";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Review/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Reviewer,SystemAdmin")]
        public async Task<IActionResult> Create(CreateReviewViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // إعادة تحميل بيانات البحث في حالة الخطأ
                    var researchQuery = new GetResearchByIdQuery(model.ResearchId, model.ReviewerId);
                    var research = await _mediator.Send(researchQuery);
                    if (research != null)
                    {
                        model.Research = _mapper.Map<ResearchSummaryDto>(research);
                    }
                    return View(model);
                }

                var user = await GetCurrentUserAsync();
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var createReviewDto = new CreateReviewDto
                {
                    ResearchId = model.ResearchId,
                    Decision = model.Decision,
                    OriginalityScore = model.OriginalityScore,
                    MethodologyScore = model.MethodologyScore,
                    ClarityScore = model.ClarityScore,
                    SignificanceScore = model.SignificanceScore,
                    ReferencesScore = model.ReferencesScore,
                    CommentsToAuthor = model.CommentsToAuthor,
                    CommentsToTrackManager = model.CommentsToTrackManager,
                    Recommendations = model.Recommendations,
                    RequiresReReview = model.RequiresReReview,
                    Deadline = model.Deadline
                };

                var command = new CreateReviewCommand
                {
                    Review = createReviewDto,
                    ReviewerId = user.Id
                };

                var reviewId = await _mediator.Send(command);

                TempData["SuccessMessage"] = "تم إرسال المراجعة بنجاح";
                return RedirectToAction("Details", new { id = reviewId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review for research {ResearchId}", model.ResearchId);
                TempData["ErrorMessage"] = "حدث خطأ في إرسال المراجعة";
                return View(model);
            }
        }

        // GET: Review/Edit/5
        [Authorize(Roles = "Reviewer,SystemAdmin")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var query = new GetReviewByIdQuery(id, user.Id);
                var review = await _mediator.Send(query);

                if (review == null)
                {
                    TempData["ErrorMessage"] = "المراجعة غير موجودة";
                    return RedirectToAction(nameof(Index));
                }

                if (!CanEditReview(review, user))
                {
                    TempData["ErrorMessage"] = "ليس لديك صلاحية لتعديل هذه المراجعة";
                    return RedirectToAction("Details", new { id });
                }

                var viewModel = new EditReviewViewModel
                {
                    Id = review.Id,
                    ResearchId = review.ResearchId,
                    Decision = review.Decision,
                    OriginalityScore = review.OriginalityScore,
                    MethodologyScore = review.MethodologyScore,
                    ClarityScore = review.ClarityScore,
                    SignificanceScore = review.SignificanceScore,
                    ReferencesScore = review.ReferencesScore,
                    CommentsToAuthor = review.CommentsToAuthor,
                    CommentsToTrackManager = review.CommentsToTrackManager,
                    Recommendations = review.Recommendations,
                    RequiresReReview = review.RequiresReReview,
                    Deadline = review.Deadline,
                    IsCompleted = review.IsCompleted,
                    CompletedDate = review.CompletedDate
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit review page for ID: {ReviewId}", id);
                TempData["ErrorMessage"] = "حدث خطأ في تحميل صفحة تعديل المراجعة";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Review/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Reviewer,SystemAdmin")]
        public async Task<IActionResult> Edit(int id, EditReviewViewModel model)
        {
            try
            {
                if (id != model.Id)
                    return NotFound();

                if (!ModelState.IsValid)
                    return View(model);

                var user = await GetCurrentUserAsync();
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var updateReviewDto = new UpdateReviewDto
                {
                    Id = model.Id,
                    ResearchId = model.ResearchId,
                    Decision = model.Decision,
                    OriginalityScore = model.OriginalityScore,
                    MethodologyScore = model.MethodologyScore,
                    ClarityScore = model.ClarityScore,
                    SignificanceScore = model.SignificanceScore,
                    ReferencesScore = model.ReferencesScore,
                    CommentsToAuthor = model.CommentsToAuthor,
                    CommentsToTrackManager = model.CommentsToTrackManager,
                    Recommendations = model.Recommendations,
                    RequiresReReview = model.RequiresReReview,
                    IsCompleted = model.IsCompleted,
                    CompletedDate = model.IsCompleted ? DateTime.UtcNow : null
                };

                var command = new UpdateReviewCommand
                {
                    Review = updateReviewDto,
                    UserId = user.Id
                };

                var result = await _mediator.Send(command);

                if (result)
                {
                    TempData["SuccessMessage"] = "تم تحديث المراجعة بنجاح";
                    return RedirectToAction("Details", new { id = model.Id });
                }
                else
                {
                    TempData["ErrorMessage"] = "فشل في تحديث المراجعة";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review {ReviewId}", id);
                TempData["ErrorMessage"] = "حدث خطأ في تحديث المراجعة";
                return View(model);
            }
        }

        // GET: Review/Pending
        [Authorize(Roles = "Reviewer,SystemAdmin")]
        public async Task<IActionResult> Pending()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var query = new GetPendingReviewsQuery(user.Id);
                var pendingReviews = await _mediator.Send(query);

                var viewModel = new PendingReviewsViewModel
                {
                    Reviews = pendingReviews,
                    CurrentUserId = user.Id
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading pending reviews for user {UserId}", GetCurrentUserId());
                TempData["ErrorMessage"] = "حدث خطأ في تحميل المراجعات المعلقة";
                return View(new PendingReviewsViewModel());
            }
        }

        // GET: Review/History
        public async Task<IActionResult> History()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var query = new GetReviewListQuery
                {
                    UserId = user.Id,
                    UserRole = user.Role,
                    PageSize = 50 // عرض المزيد في صفحة التاريخ
                };

                var reviews = await _mediator.Send(query);
                var statistics = await GetReviewerStatistics(user.Id);

                var viewModel = new ReviewHistoryViewModel
                {
                    Reviews = reviews.Items.Where(r => r.IsCompleted).ToList(),
                    Statistics = statistics,
                    CurrentUserId = user.Id
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading review history for user {UserId}", GetCurrentUserId());
                TempData["ErrorMessage"] = "حدث خطأ في تحميل تاريخ المراجعات";
                return View(new ReviewHistoryViewModel());
            }
        }

        #region Helper Methods

        private async Task<ReviewStatisticsDto> GetReviewStatistics(string userId, UserRole userRole)
        {
            var query = new GetReviewListQuery
            {
                UserId = userId,
                UserRole = userRole,
                PageSize = int.MaxValue // جلب جميع المراجعات للإحصائيات
            };

            var result = await _mediator.Send(query);
            var reviews = result.Items;

            return new ReviewStatisticsDto
            {
                TotalReviews = reviews.Count,
                CompletedReviews = reviews.Count(r => r.IsCompleted),
                PendingReviews = reviews.Count(r => !r.IsCompleted),
                OverdueReviews = reviews.Count(r => !r.IsCompleted && r.Deadline < DateTime.UtcNow),
                AverageScore = reviews.Where(r => r.IsCompleted && r.OverallScore > 0)
                                    .DefaultIfEmpty()
                                    .Average(r => (double)(r?.OverallScore ?? 0))
            };
        }

        private async Task<ReviewerStatisticsDto> GetReviewerStatistics(string reviewerId)
        {
            var query = new GetReviewListQuery
            {
                UserId = reviewerId,
                UserRole = UserRole.Reviewer,
                PageSize = int.MaxValue
            };

            var result = await _mediator.Send(query);
            var reviews = result.Items;
            var completedReviews = reviews.Where(r => r.IsCompleted).ToList();

            return new ReviewerStatisticsDto
            {
                ReviewerId = reviewerId,
                TotalAssigned = reviews.Count,
                CompletedReviews = completedReviews.Count,
                AverageReviewTime = completedReviews.Any()
                    ? completedReviews.Where(r => r.CompletedDate.HasValue)
                                    .Average(r => (r.CompletedDate!.Value - r.AssignedDate).TotalDays)
                    : 0,
                AcceptanceRate = completedReviews.Any()
                    ? (double)completedReviews.Count(r => r.Decision == ReviewDecision.AcceptAsIs ||
                                                         r.Decision == ReviewDecision.AcceptWithMinorRevisions) /
                      completedReviews.Count * 100
                    : 0,
                AverageScore = completedReviews.Any()
                    ? (double)completedReviews.Average(r => r.OverallScore)
                    : 0
            };
        }

        private List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> GetTrackOptions()
        {
            return Enum.GetValues<ResearchTrack>()
                .Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = ((int)t).ToString(),
                    Text = GetTrackDisplayName(t)
                }).ToList();
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

        private static bool CanEditReview(ReviewDto review, User user)
        {
            if (user.Role == UserRole.SystemAdmin) return true;
            return review.ReviewerId == user.Id && !review.IsCompleted;
        }

        private static bool CanDeleteReview(ReviewDto review, User user)
        {
            if (user.Role == UserRole.SystemAdmin) return true;
            return review.ReviewerId == user.Id && !review.IsCompleted;
        }

        #endregion
    }
}