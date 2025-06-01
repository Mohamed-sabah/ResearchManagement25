using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;
using ResearchManagement.Application.Interfaces;

namespace ResearchManagement.Web.Controllers
{
    [Authorize]
    public class DashboardController : BaseController
    {
        private readonly IResearchRepository _researchRepository;
        private readonly IReviewRepository _reviewRepository;

        public DashboardController(
            UserManager<User> userManager,
            IResearchRepository researchRepository,
            IReviewRepository reviewRepository) : base(userManager)
        {
            _researchRepository = researchRepository;
            _reviewRepository = reviewRepository;
        }

        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction("Login", "Account");

            ViewData["UserRole"] = user.Role;
            ViewData["UserName"] = $"{user.FirstName} {user.LastName}";

            switch (user.Role)
            {
                case UserRole.Researcher:
                    return await ResearcherDashboard();
                case UserRole.Reviewer:
                    return await ReviewerDashboard();
                case UserRole.TrackManager:
                    return await TrackManagerDashboard();
                case UserRole.ConferenceManager:
                    return await ConferenceManagerDashboard();
                default:
                    return View("Error");
            }
        }

        private async Task<IActionResult> ResearcherDashboard()
        {
            var userId = GetCurrentUserId();
            var researches = await _researchRepository.GetByUserIdAsync(userId);

            var dashboardData = new
            {
                TotalResearches = researches.Count(),
                AcceptedResearches = researches.Count(r => r.Status == ResearchStatus.Accepted),
                PendingResearches = researches.Count(r => r.Status == ResearchStatus.Submitted ||
                                                         r.Status == ResearchStatus.UnderReview),
                RejectedResearches = researches.Count(r => r.Status == ResearchStatus.Rejected),
                RecentResearches = researches.OrderByDescending(r => r.SubmissionDate).Take(5)
            };

            return View("ResearcherDashboard", dashboardData);
        }

        private async Task<IActionResult> ReviewerDashboard()
        {
            var userId = GetCurrentUserId();
            var reviews = await _reviewRepository.GetByReviewerIdAsync(userId);
            var pendingReviews = await _reviewRepository.GetPendingReviewsAsync(userId);

            var dashboardData = new
            {
                TotalReviews = reviews.Count(),
                CompletedReviews = reviews.Count(r => r.IsCompleted),
                PendingReviews = pendingReviews.Count(),
                OverdueReviews = pendingReviews.Count(r => r.Deadline < DateTime.UtcNow),
                RecentReviews = reviews.OrderByDescending(r => r.AssignedDate).Take(5)
            };

            return View("ReviewerDashboard", dashboardData);
        }

        private async Task<IActionResult> TrackManagerDashboard()
        {
            // TODO: Implement track manager specific logic
            return View("TrackManagerDashboard");
        }

        private async Task<IActionResult> ConferenceManagerDashboard()
        {
            var allResearches = await _researchRepository.GetAllAsync();

            var dashboardData = new
            {
                TotalResearches = allResearches.Count(),
                AcceptedResearches = allResearches.Count(r => r.Status == ResearchStatus.Accepted),
                PendingResearches = allResearches.Count(r => r.Status == ResearchStatus.UnderReview ||
                                                            r.Status == ResearchStatus.UnderEvaluation),
                RejectedResearches = allResearches.Count(r => r.Status == ResearchStatus.Rejected),
                RecentSubmissions = allResearches.OrderByDescending(r => r.SubmissionDate).Take(10)
            };

            return View("ConferenceManagerDashboard", dashboardData);
        }
    }
}
