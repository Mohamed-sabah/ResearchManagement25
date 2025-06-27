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
using AutoMapper;
using MediatR;
using ResearchManagement.Application.Commands.Review;
using ResearchManagement.Application.Queries.Research;
using ResearchManagement.Application.Queries.TrackManager;

namespace ResearchManagement.Web.Controllers
{
    [Authorize(Roles = "TrackManager,SystemAdmin")]
    public class TrackManagerController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<TrackManagerController> _logger;

        public TrackManagerController(
            UserManager<User> userManager,
            IMediator mediator,
            IMapper mapper,
            ILogger<TrackManagerController> logger) : base(userManager)
        {
            _mediator = mediator;
            _mapper = mapper;
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

                var query = new GetTrackDashboardQuery(user.Id);
                var dashboard = await _mediator.Send(query);

                var viewModel = new TrackManagerDashboardViewModel
                {
                    TrackName = dashboard.TrackName,
                    Track = dashboard.Track,
                    Statistics = dashboard.Statistics,
                    RecentResearches = dashboard.RecentResearches,
                    PendingResearches = dashboard.PendingResearches,
                    ResearchesNeedingReviewers = dashboard.ResearchesNeedingReviewers,
                    OverdueReviews = dashboard.OverdueReviews
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading track manager dashboard for user {UserId}", GetCurrentUserId());
                TempData["ErrorMessage"] = "حدث خطأ في تحميل لوحة تحكم مدير المسار";
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
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var query = new GetTrackResearchesQuery
                {
                    TrackManagerId = user.Id,
                    SearchTerm = searchTerm,
                    Status = status,
                    FromDate = fromDate,
                    ToDate = toDate,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _mediator.Send(query);

                var viewModel = new TrackResearchesViewModel
                {
                    Researches = _mapper.Map<List<Research>>(result.Items),
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
                        TotalCount = result.TotalCount,
                        TotalPages = result.TotalPages
                    }
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading track researches for user {UserId}", GetCurrentUserId());
                TempData["ErrorMessage"] = "حدث خطأ في تحميل بحوث المسار";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: TrackManager/AssignReviewers/5
        public async Task<IActionResult> AssignReviewers(int researchId)
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
                    TempData["ErrorMessage"] = "البحث غير موجود أو لا ينتمي لمسارك";
                    return RedirectToAction(nameof(Researches));
                }

                // جلب المراجعين المتاحين
                var reviewersQuery = new GetAvailableReviewersQuery(user.Id, researchId);
                var availableReviewers = await _mediator.Send(reviewersQuery);

                var viewModel = new AssignReviewersViewModel
                {
                    Research = _mapper.Map<Research>(research),
                    AvailableReviewers = _mapper.Map<List<User>>(availableReviewers),
                    CurrentReviews = research.Reviews?.ToList() ?? new List<ReviewDto>()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading assign reviewers page for research {ResearchId}", researchId);
                TempData["ErrorMessage"] = "حدث خطأ في تحميل صفحة تعيين المراجعين";
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
                if (user == null)
                    return Json(new { success = false, message = "غير مصرح له" });

                var command = new AssignReviewerCommand
                {
                    ResearchId = researchId,
                    ReviewerId = reviewerId,
                    TrackManagerId = user.Id,
                    Deadline = deadline
                };

                var reviewId = await _mediator.Send(command);

                return Json(new { success = true, message = "تم تعيين المراجع بنجاح", reviewId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning reviewer {ReviewerId} to research {ResearchId}",
                    reviewerId, researchId);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: TrackManager/RemoveReviewer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveReviewer(int reviewId, string? reason)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return Json(new { success = false, message = "غير مصرح له" });

                var command = new RemoveReviewerCommand
                {
                    ReviewId = reviewId,
                    TrackManagerId = user.Id,
                    Reason = reason
                };

                var result = await _mediator.Send(command);

                if (result)
                {
                    return Json(new { success = true, message = "تم إزالة المراجع بنجاح" });
                }
                else
                {
                    return Json(new { success = false, message = "فشل في إزالة المراجع" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing reviewer {ReviewId}", reviewId);
                return Json(new { success = false, message = ex.Message });
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
                if (user == null)
                    return Json(new { success = false, message = "غير مصرح له" });

                var command = new UpdateResearchStatusCommand
                {
                    ResearchId = researchId,
                    NewStatus = newStatus,
                    Notes = notes,
                    TrackManagerId = user.Id
                };

                var result = await _mediator.Send(command);

                if (result)
                {
                    return Json(new { success = true, message = "تم تحديث حالة البحث بنجاح" });
                }
                else
                {
                    return Json(new { success = false, message = "فشل في تحديث حالة البحث" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating research status for {ResearchId}", researchId);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: TrackManager/ReviewerManagement
        public async Task<IActionResult> ReviewerManagement()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var query = new GetReviewerStatisticsQuery(user.Id);
                var statistics = await _mediator.Send(query);

                var viewModel = new ReviewerManagementViewModel
                {
                    ReviewerStatistics = statistics
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reviewer management for user {UserId}", GetCurrentUserId());
                TempData["ErrorMessage"] = "حدث خطأ في تحميل إدارة المراجعين";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: TrackManager/Reports
        public async Task<IActionResult> Reports(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var query = new GetTrackReportsQuery(user.Id, fromDate, toDate);
                var report = await _mediator.Send(query);

                return View(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating track reports for user {UserId}", GetCurrentUserId());
                TempData["ErrorMessage"] = "حدث خطأ في إنشاء التقارير";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}