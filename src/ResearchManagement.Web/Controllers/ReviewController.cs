using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Commands.Review;
using ResearchManagement.Application.Queries.Review;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Web.Models.ViewModels.Review;
using ResearchManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ResearchManagement.Web.Controllers
{
    [Authorize(Roles = "Reviewer,TrackManager,SystemAdmin")]
    public class ReviewController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IReviewRepository _reviewRepository;
        private readonly IResearchRepository _researchRepository;
        private readonly ILogger<ReviewController> _logger;
        private readonly ApplicationDbContext _context;

        public ReviewController(
            UserManager<User> userManager,
            IMediator mediator,
            IReviewRepository reviewRepository,
            IResearchRepository researchRepository,
            ILogger<ReviewController> logger,
            ApplicationDbContext context) : base(userManager)
        {
            _mediator = mediator;
            _reviewRepository = reviewRepository;
            _researchRepository = researchRepository;
            _logger = logger;
            _context = context;
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

                var query = new GetReviewListQuery(user.Id, user.Role)
                {
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
                AddErrorMessage("حدث خطأ في تحميل المراجعات");
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
                    AddErrorMessage("المراجعة غير موجودة أو ليس لديك صلاحية للوصول إليها");
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
                AddErrorMessage("حدث خطأ في تحميل تفاصيل المراجعة");
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

                // التحقق من أن البحث موجود ومخصص للمراجع
                var research = await _context.Researches
                    .Include(r => r.Authors)
                    .Include(r => r.Files)
                    .FirstOrDefaultAsync(r => r.Id == researchId);

                if (research == null)
                {
                    AddErrorMessage("البحث غير موجود");
                    return RedirectToAction(nameof(Index));
                }

                // التحقق من وجود مراجعة سابقة للمستخدم
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.ResearchId == researchId && r.ReviewerId == user.Id);

                if (existingReview != null)
                {
                    if (existingReview.IsCompleted)
                    {
                        AddWarningMessage("لقد قمت بإكمال مراجعة هذا البحث مسبقاً");
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
                    Research = new ResearchSummaryDto
                    {
                        Id = research.Id,
                        Title = research.Title,
                        TitleEn = research.TitleEn,
                        AbstractAr = research.AbstractAr,
                        AbstractEn = research.AbstractEn,
                        Track = research.Track,
                        ResearchType = research.ResearchType,
                        SubmissionDate = research.SubmissionDate,
                        Authors = research.Authors.Select(a => new ResearchAuthorDto
                        {
                            FirstName = a.FirstName,
                            LastName = a.LastName,
                            Email = a.Email,
                            Institution = a.Institution,
                            IsCorresponding = a.IsCorresponding,
                            Order = a.Order
                        }).ToList(),
                        Files = research.Files.Where(f => f.IsActive).Select(f => new ResearchFileDto
                        {
                            Id = f.Id,
                            FileName = f.FileName,
                            OriginalFileName = f.OriginalFileName,
                            FileSize = f.FileSize,
                            ContentType = f.ContentType,
                            Description = f.Description
                        }).ToList()
                    },
                    Deadline = DateTime.UtcNow.AddDays(14) // موعد نهائي افتراضي 14 يوم
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create review page for research {ResearchId}", researchId);
                AddErrorMessage("حدث خطأ في تحميل صفحة المراجعة");
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
                    // إعادة تحميل بيانات البحث
                    var research = await _context.Researches
                        .Include(r => r.Authors)
                        .Include(r => r.Files)
                        .FirstOrDefaultAsync(r => r.Id == model.ResearchId);

                    if (research != null)
                    {
                        model.Research = MapResearchToDto(research);
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

                AddSuccessMessage("تم إرسال المراجعة بنجاح");
                return RedirectToAction("Details", new { id = reviewId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review for research {ResearchId}", model.ResearchId);
                AddErrorMessage("حدث خطأ في إرسال المراجعة");
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

                var review = await _reviewRepository.GetByIdWithDetailsAsync(id);
                if (review == null)
                {
                    AddErrorMessage("المراجعة غير موجودة");
                    return RedirectToAction(nameof(Index));
                }

                if (!CanEditReview(review, user))
                {
                    AddErrorMessage("ليس لديك صلاحية لتعديل هذه المراجعة");
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
                AddErrorMessage("حدث خطأ في تحميل صفحة تعديل المراجعة");
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
                    AddSuccessMessage("تم تحديث المراجعة بنجاح");
                    return RedirectToAction("Details", new { id = model.Id });
                }
                else
                {
                    AddErrorMessage("فشل في تحديث المراجعة");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review {ReviewId}", id);
                AddErrorMessage("حدث خطأ في تحديث المراجعة");
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

                var pendingReviews = await _reviewRepository.GetPendingReviewsAsync(user.Id);

                var viewModel = new PendingReviewsViewModel
                {
                    Reviews = pendingReviews.ToList(),
                    CurrentUserId = user.Id
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading pending reviews for user {UserId}", GetCurrentUserId());
                AddErrorMessage("حدث خطأ في تحميل المراجعات المعلقة");
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

                var reviews = await _reviewRepository.GetByReviewerIdAsync(user.Id);
                var statistics = await GetReviewerStatistics(user.Id);

                var viewModel = new ReviewHistoryViewModel
                {
                    Reviews = reviews.OrderByDescending(r => r.CompletedDate ?? r.AssignedDate).ToList(),
                    Statistics = statistics,
                    CurrentUserId = user.Id
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading review history for user {UserId}", GetCurrentUserId());
                AddErrorMessage("حدث خطأ في تحميل تاريخ المراجعات");
                return View(new ReviewHistoryViewModel());
            }
        }

        // POST: Review/MarkAsCompleted/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Reviewer,SystemAdmin")]
        public async Task<IActionResult> MarkAsCompleted(int id)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return Json(new { success = false, message = "غير مصرح له" });

                var command = new CompleteReviewCommand
                {
                    ReviewId = id,
                    UserId = user.Id
                };

                var result = await _mediator.Send(command);

                if (result)
                {
                    return Json(new { success = true, message = "تم تسليم المراجعة بنجاح" });
                }
                else
                {
                    return Json(new { success = false, message = "فشل في تسليم المراجعة" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing review {ReviewId}", id);
                return Json(new { success = false, message = "حدث خطأ في تسليم المراجعة" });
            }
        }

        #region Helper Methods

        private async Task<ReviewStatisticsDto> GetReviewStatistics(string userId, UserRole userRole)
        {
            var reviews = userRole == UserRole.Reviewer
                ? await _reviewRepository.GetByReviewerIdAsync(userId)
                : await _reviewRepository.GetAllAsync();

            return new ReviewStatisticsDto
            {
                TotalReviews = reviews.Count(),
                CompletedReviews = reviews.Count(r => r.IsCompleted),
                PendingReviews = reviews.Count(r => !r.IsCompleted),
                OverdueReviews = reviews.Count(r => !r.IsCompleted && r.Deadline < DateTime.UtcNow),
                AverageScore = reviews.Where(r => r.IsCompleted && r.OverallScore > 0)
                                    .DefaultIfEmpty()
                                    .Average(r => r?.OverallScore ?? 0)
            };
        }

        private async Task<ReviewerStatisticsDto> GetReviewerStatistics(string reviewerId)
        {
            var reviews = await _reviewRepository.GetByReviewerIdAsync(reviewerId);
            var completedReviews = reviews.Where(r => r.IsCompleted).ToList();

            return new ReviewerStatisticsDto
            {
                TotalReviews = reviews.Count(),
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

        private ResearchSummaryDto MapResearchToDto(Research research)
        {
            return new ResearchSummaryDto
            {
                Id = research.Id,
                Title = research.Title,
                TitleEn = research.TitleEn,
                AbstractAr = research.AbstractAr,
                AbstractEn = research.AbstractEn,
                Track = research.Track,
                ResearchType = research.ResearchType,
                SubmissionDate = research.SubmissionDate,
                Authors = research.Authors.Select(a => new ResearchAuthorDto
                {
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    Email = a.Email,
                    Institution = a.Institution,
                    IsCorresponding = a.IsCorresponding,
                    Order = a.Order
                }).ToList(),
                Files = research.Files.Where(f => f.IsActive).Select(f => new ResearchFileDto
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    OriginalFileName = f.OriginalFileName,
                    FileSize = f.FileSize,
                    ContentType = f.ContentType,
                    Description = f.Description
                }).ToList()
            };
        }

        #endregion
    }
}



//using Microsoft.AspNetCore.Mvc;

//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Authorization;
//using MediatR;
//using ResearchManagement.Domain.Entities;
//using ResearchManagement.Domain.Enums;
//using ResearchManagement.Application.DTOs;
//using ResearchManagement.Application.Commands.Review;
//using ResearchManagement.Application.Interfaces;
//namespace ResearchManagement.Web.Controllers
//{
//    [Authorize(Roles = "Reviewer,TrackManager")]
//    public class ReviewController : BaseController
//    {
//        private readonly IMediator _mediator;
//        private readonly IReviewRepository _reviewRepository;
//        private readonly IResearchRepository _researchRepository;
//        private readonly ILogger<ResearchController> _logger;

//        public ReviewController(
//            UserManager<User> userManager,
//            IMediator mediator,
//            IReviewRepository reviewRepository,
//            IResearchRepository researchRepository,
//            ILogger<ResearchController> logger) : base(userManager)
//        {
//            _mediator = mediator;
//            _reviewRepository = reviewRepository;
//            _researchRepository = researchRepository;
//            _logger = logger;
//        }

//        public async Task<IActionResult> Index()
//        {
//            var userId = GetCurrentUserId();
//            var reviews = await _reviewRepository.GetByReviewerIdAsync(userId);
//            return View(reviews);
//        }

//        [HttpGet]
//        public async Task<IActionResult> Details(int id)
//        {
//            var review = await _reviewRepository.GetByIdWithDetailsAsync(id);
//            if (review == null)
//                return NotFound();

//            var user = await GetCurrentUserAsync();
//            if (user == null)
//                return RedirectToAction("Login", "Account");

//            // التحقق من الصلاحيات
//            if (user.Role == UserRole.Reviewer && review.ReviewerId != user.Id)
//                return Forbid();

//            return View(review);
//        }

//        [HttpGet]
//        [Authorize(Roles = "Reviewer")]
//        public async Task<IActionResult> Create(int researchId)
//        {
//            var research = await _researchRepository.GetByIdWithDetailsAsync(researchId);
//            if (research == null)
//                return NotFound();

//            // التحقق من وجود مراجعة سابقة
//            var existingReview = (await _reviewRepository.GetByResearchIdAsync(researchId))
//                .FirstOrDefault(r => r.ReviewerId == GetCurrentUserId());

//            if (existingReview != null)
//            {
//                return RedirectToAction("Edit", new { id = existingReview.Id });
//            }

//            var model = new CreateReviewDto
//            {
//                ResearchId = researchId
//            };

//            ViewData["Research"] = research;
//            return View(model);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [Authorize(Roles = "Reviewer")]
//        public async Task<IActionResult> Create(CreateReviewDto model)
//        {
//            if (!ModelState.IsValid)
//            {
//                var research = await _researchRepository.GetByIdWithDetailsAsync(model.ResearchId);
//                ViewData["Research"] = research;
//                return View(model);
//            }

//            try
//            {
//                // التحقق من وجود مراجعة سابقة
//                var existingReviews = await _reviewRepository.GetByResearchIdAsync(model.ResearchId);
//                var userReview = existingReviews.FirstOrDefault(r => r.ReviewerId == GetCurrentUserId());

//                if (userReview != null && userReview.IsCompleted)
//                {
//                    AddErrorMessage("لقد تم إكمال مراجعة هذا البحث مسبقاً");
//                    return RedirectToAction("Index");
//                }

//                var command = new CreateReviewCommand
//                {
//                    Review = model,
//                    ReviewerId = GetCurrentUserId()
//                };

//                var reviewId = await _mediator.Send(command);

//                AddSuccessMessage("تم إرسال المراجعة بنجاح");
//                return RedirectToAction("Details", new { id = reviewId });
//            }
//            catch (Exception ex)
//            {
//                _logger?.LogError(ex, "Error creating review for research {ResearchId}", model.ResearchId);
//                AddErrorMessage($"حدث خطأ في إرسال المراجعة: {ex.Message}");

//                var research = await _researchRepository.GetByIdWithDetailsAsync(model.ResearchId);
//                ViewData["Research"] = research;
//                return View(model);
//            }
//        }

//        [HttpGet]
//        [Authorize(Roles = "Reviewer")]
//        public async Task<IActionResult> Edit(int id)
//        {
//            var review = await _reviewRepository.GetByIdWithDetailsAsync(id);
//            if (review == null)
//                return NotFound();

//            // التحقق من الملكية
//            if (review.ReviewerId != GetCurrentUserId())
//                return Forbid();

//            // التحقق من إمكانية التعديل
//            if (review.IsCompleted && !review.RequiresReReview)
//            {
//                AddWarningMessage("لا يمكن تعديل المراجعة بعد الانتهاء منها");
//                return RedirectToAction("Details", new { id });
//            }

//            var model = new UpdateReviewDto
//            {
//                Id = review.Id,
//                Decision = review.Decision,
//                OriginalityScore = review.OriginalityScore,
//                MethodologyScore = review.MethodologyScore,
//                ClarityScore = review.ClarityScore,
//                SignificanceScore = review.SignificanceScore,
//                ReferencesScore = review.ReferencesScore,
//                CommentsToAuthor = review.CommentsToAuthor,
//                CommentsToTrackManager = review.CommentsToTrackManager,
//                RequiresReReview = review.RequiresReReview,
//                ResearchId = review.ResearchId
//            };

//            ViewData["Research"] = review.Research;
//            return View(model);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [Authorize(Roles = "Reviewer")]
//        public async Task<IActionResult> Edit(UpdateReviewDto model)
//        {
//            if (!ModelState.IsValid)
//            {
//                var research = await _researchRepository.GetByIdWithDetailsAsync(model.ResearchId);
//                ViewData["Research"] = research;
//                return View(model);
//            }

//            try
//            {
//                var review = await _reviewRepository.GetByIdAsync(model.Id);
//                if (review == null)
//                    return NotFound();

//                // التحقق من الملكية
//                if (review.ReviewerId != GetCurrentUserId())
//                    return Forbid();

//                // تحديث البيانات
//                review.Decision = model.Decision;
//                review.OriginalityScore = model.OriginalityScore;
//                review.MethodologyScore = model.MethodologyScore;
//                review.ClarityScore = model.ClarityScore;
//                review.SignificanceScore = model.SignificanceScore;
//                review.ReferencesScore = model.ReferencesScore;
//                review.CommentsToAuthor = model.CommentsToAuthor;
//                review.CommentsToTrackManager = model.CommentsToTrackManager;
//                review.RequiresReReview = model.RequiresReReview;
//                review.CompletedDate = DateTime.UtcNow;
//                review.IsCompleted = true;
//                review.UpdatedAt = DateTime.UtcNow;
//                review.UpdatedBy = GetCurrentUserId();

//                await _reviewRepository.UpdateAsync(review);

//                AddSuccessMessage("تم تحديث المراجعة بنجاح");
//                return RedirectToAction("Details", new { id = model.Id });
//            }
//            catch (Exception ex)
//            {
//                AddErrorMessage($"حدث خطأ في تحديث المراجعة: {ex.Message}");
//                var research = await _researchRepository.GetByIdWithDetailsAsync(model.ResearchId);
//                ViewData["Research"] = research;
//                return View(model);
//            }
//        }
//    }
//}
