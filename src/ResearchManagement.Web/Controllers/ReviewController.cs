using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Commands.Review;
using ResearchManagement.Application.Interfaces;
namespace ResearchManagement.Web.Controllers
{
    [Authorize(Roles = "Reviewer,TrackManager")]
    public class ReviewController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IReviewRepository _reviewRepository;
        private readonly IResearchRepository _researchRepository;

        public ReviewController(
            UserManager<User> userManager,
            IMediator mediator,
            IReviewRepository reviewRepository,
            IResearchRepository researchRepository) : base(userManager)
        {
            _mediator = mediator;
            _reviewRepository = reviewRepository;
            _researchRepository = researchRepository;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var reviews = await _reviewRepository.GetByReviewerIdAsync(userId);
            return View(reviews);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var review = await _reviewRepository.GetByIdWithDetailsAsync(id);
            if (review == null)
                return NotFound();

            var user = await GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction("Login", "Account");

            // التحقق من الصلاحيات
            if (user.Role == UserRole.Reviewer && review.ReviewerId != user.Id)
                return Forbid();

            return View(review);
        }

        [HttpGet]
        [Authorize(Roles = "Reviewer")]
        public async Task<IActionResult> Create(int researchId)
        {
            var research = await _researchRepository.GetByIdWithDetailsAsync(researchId);
            if (research == null)
                return NotFound();

            // التحقق من وجود مراجعة سابقة
            var existingReview = (await _reviewRepository.GetByResearchIdAsync(researchId))
                .FirstOrDefault(r => r.ReviewerId == GetCurrentUserId());

            if (existingReview != null)
            {
                return RedirectToAction("Edit", new { id = existingReview.Id });
            }

            var model = new CreateReviewDto
            {
                ResearchId = researchId
            };

            ViewData["Research"] = research;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Reviewer")]
        public async Task<IActionResult> Create(CreateReviewDto model)
        {
            if (!ModelState.IsValid)
            {
                var research = await _researchRepository.GetByIdWithDetailsAsync(model.ResearchId);
                ViewData["Research"] = research;
                return View(model);
            }

            try
            {
                var command = new CreateReviewCommand
                {
                    Review = model,
                    ReviewerId = GetCurrentUserId()
                };

                var reviewId = await _mediator.Send(command);

                AddSuccessMessage("تم إرسال المراجعة بنجاح");
                return RedirectToAction("Details", new { id = reviewId });
            }
            catch (Exception ex)
            {
                AddErrorMessage($"حدث خطأ في إرسال المراجعة: {ex.Message}");
                var research = await _researchRepository.GetByIdWithDetailsAsync(model.ResearchId);
                ViewData["Research"] = research;
                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Reviewer")]
        public async Task<IActionResult> Edit(int id)
        {
            var review = await _reviewRepository.GetByIdWithDetailsAsync(id);
            if (review == null)
                return NotFound();

            // التحقق من الملكية
            if (review.ReviewerId != GetCurrentUserId())
                return Forbid();

            // التحقق من إمكانية التعديل
            if (review.IsCompleted && !review.RequiresReReview)
            {
                AddWarningMessage("لا يمكن تعديل المراجعة بعد الانتهاء منها");
                return RedirectToAction("Details", new { id });
            }

            var model = new UpdateReviewDto
            {
                Id = review.Id,
                Decision = review.Decision,
                OriginalityScore = review.OriginalityScore,
                MethodologyScore = review.MethodologyScore,
                ClarityScore = review.ClarityScore,
                SignificanceScore = review.SignificanceScore,
                ReferencesScore = review.ReferencesScore,
                CommentsToAuthor = review.CommentsToAuthor,
                CommentsToTrackManager = review.CommentsToTrackManager,
                RequiresReReview = review.RequiresReReview,
                ResearchId = review.ResearchId
            };

            ViewData["Research"] = review.Research;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Reviewer")]
        public async Task<IActionResult> Edit(UpdateReviewDto model)
        {
            if (!ModelState.IsValid)
            {
                var research = await _researchRepository.GetByIdWithDetailsAsync(model.ResearchId);
                ViewData["Research"] = research;
                return View(model);
            }

            try
            {
                var review = await _reviewRepository.GetByIdAsync(model.Id);
                if (review == null)
                    return NotFound();

                // التحقق من الملكية
                if (review.ReviewerId != GetCurrentUserId())
                    return Forbid();

                // تحديث البيانات
                review.Decision = model.Decision;
                review.OriginalityScore = model.OriginalityScore;
                review.MethodologyScore = model.MethodologyScore;
                review.ClarityScore = model.ClarityScore;
                review.SignificanceScore = model.SignificanceScore;
                review.ReferencesScore = model.ReferencesScore;
                review.CommentsToAuthor = model.CommentsToAuthor;
                review.CommentsToTrackManager = model.CommentsToTrackManager;
                review.RequiresReReview = model.RequiresReReview;
                review.CompletedDate = DateTime.UtcNow;
                review.IsCompleted = true;
                review.UpdatedAt = DateTime.UtcNow;
                review.UpdatedBy = GetCurrentUserId();

                await _reviewRepository.UpdateAsync(review);

                AddSuccessMessage("تم تحديث المراجعة بنجاح");
                return RedirectToAction("Details", new { id = model.Id });
            }
            catch (Exception ex)
            {
                AddErrorMessage($"حدث خطأ في تحديث المراجعة: {ex.Message}");
                var research = await _researchRepository.GetByIdWithDetailsAsync(model.ResearchId);
                ViewData["Research"] = research;
                return View(model);
            }
        }
    }
}
