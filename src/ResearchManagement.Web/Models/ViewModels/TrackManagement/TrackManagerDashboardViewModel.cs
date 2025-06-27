using MediatR;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ResearchManagement.Web.Models.ViewModels.TrackManager
{
    // Track Manager Dashboard ViewModel
    public class TrackManagerDashboardViewModel
    {
        public string TrackName { get; set; } = string.Empty;
        public ResearchTrack Track { get; set; }
        public TrackStatisticsDto Statistics { get; set; } = new();
        public List<Domain.Entities.Research> RecentResearches { get; set; } = new();
        public List<Domain.Entities.Research> PendingResearches { get; set; } = new();
        public List<Domain.Entities.Research> ResearchesNeedingReviewers { get; set; } = new();
        public List<Domain.Entities.Review> OverdueReviews { get; set; } = new();

        // Quick Stats
        public int TotalResearches => Statistics.TotalResearches;
        public int TotalReviewers { get; set; }
        public int PendingAssignments => PendingResearches.Count;
        public int CompletedReviews { get; set; }
    }

    // Track Researches ViewModel
    public class TrackResearchesViewModel
    {
        public List<Domain.Entities.Research> Researches { get; set; } = new();
        public string TrackName { get; set; } = string.Empty;
        public ResearchTrack Track { get; set; }
        public ResearchFilterDto Filter { get; set; } = new();
        public PaginationDto Pagination { get; set; } = new();
    }

    // Assign Reviewers ViewModel
    public class AssignReviewersViewModel
    {
        public Domain.Entities.Research Research { get; set; } = new();
        public List<User> AvailableReviewers { get; set; } = new();
        public List<Domain.Entities.Review> CurrentReviews { get; set; } = new();
        public string TrackName { get; set; } = string.Empty;

        [Display(Name = "المراجع")]
        [Required(ErrorMessage = "يرجى اختيار المراجع")]
        public string SelectedReviewerId { get; set; } = string.Empty;

        [Display(Name = "الموعد النهائي")]
        public DateTime? ReviewDeadline { get; set; }

        [Display(Name = "ملاحظات")]
        [StringLength(500)]
        public string? Notes { get; set; }
    }

    // Reviewer Management ViewModel
    public class ReviewerManagementViewModel
    {
        public string TrackName { get; set; } = string.Empty;
        public ResearchTrack Track { get; set; }
        public List<ReviewerStatisticsDto> ReviewerStatistics { get; set; } = new();
        public List<User> AvailableReviewers { get; set; } = new();
    }

    // Research Status Update ViewModel
    public class ResearchStatusUpdateViewModel
    {
        public int ResearchId { get; set; }

        [Display(Name = "البحث")]
        public string ResearchTitle { get; set; } = string.Empty;

        [Display(Name = "الحالة الحالية")]
        public ResearchStatus CurrentStatus { get; set; }

        [Required(ErrorMessage = "يرجى اختيار الحالة الجديدة")]
        [Display(Name = "الحالة الجديدة")]
        public ResearchStatus NewStatus { get; set; }

        [Display(Name = "ملاحظات")]
        [StringLength(1000, ErrorMessage = "الملاحظات يجب ألا تتجاوز 1000 حرف")]
        public string? Notes { get; set; }

        [Display(Name = "إرسال إشعار للباحث")]
        public bool SendNotification { get; set; } = true;
    }

    // Track Reports ViewModel
    public class TrackReportsViewModel
    {
        public string TrackName { get; set; } = string.Empty;
        public ResearchTrack Track { get; set; }
        public TrackReportDto Report { get; set; } = new();
        public DateTime ReportDate { get; set; } = DateTime.UtcNow;
    }

    // Review Assignment Details ViewModel
    public class ReviewAssignmentDetailsViewModel
    {
        public Domain.Entities.Research Research { get; set; } = new();
        public List<ReviewAssignmentDto> Assignments { get; set; } = new();
        public string TrackName { get; set; } = string.Empty;
        public bool CanModifyAssignments { get; set; }
    }

    // Bulk Actions ViewModel
    public class BulkActionsViewModel
    {
        [Required(ErrorMessage = "يرجى اختيار البحوث")]
        public List<int> SelectedResearchIds { get; set; } = new();

        [Required(ErrorMessage = "يرجى اختيار الإجراء")]
        [Display(Name = "الإجراء")]
        public BulkActionType Action { get; set; }

        [Display(Name = "الحالة الجديدة")]
        public ResearchStatus? NewStatus { get; set; }

        [Display(Name = "المراجع")]
        public string? ReviewerId { get; set; }

        [Display(Name = "الموعد النهائي")]
        public DateTime? Deadline { get; set; }

        [Display(Name = "ملاحظات")]
        [StringLength(500)]
        public string? Notes { get; set; }
    }

    // Review Performance ViewModel
    public class ReviewPerformanceViewModel
    {
        public string TrackName { get; set; } = string.Empty;
        public List<ReviewerPerformanceDto> ReviewerPerformances { get; set; } = new();
        public DateTime FromDate { get; set; } = DateTime.UtcNow.AddMonths(-6);
        public DateTime ToDate { get; set; } = DateTime.UtcNow;
    }
}

// Supporting DTOs
namespace ResearchManagement.Application.DTOs
{
    public class TrackStatisticsDto
    {
        public int TotalResearches { get; set; }
        public int SubmittedResearches { get; set; }
        public int UnderReviewResearches { get; set; }
        public int AcceptedResearches { get; set; }
        public int RejectedResearches { get; set; }
        public int RequiringRevisionResearches { get; set; }
        public double AverageProcessingTime { get; set; }

        public double AcceptanceRate => TotalResearches > 0 ?
            (double)AcceptedResearches / TotalResearches * 100 : 0;

        public double RejectionRate => TotalResearches > 0 ?
            (double)RejectedResearches / TotalResearches * 100 : 0;
    }

    public class ResearchFilterDto
    {
        public string? SearchTerm { get; set; }
        public ResearchStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SubmittedBy { get; set; }
        public bool? HasReviews { get; set; }
    }

    public class PaginationDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    public class TrackReportDto
    {
        public string TrackName { get; set; } = string.Empty;
        public DateTime ReportGeneratedAt { get; set; }
        public int TotalResearches { get; set; }
        public Dictionary<ResearchStatus, int> ResearchesByStatus { get; set; } = new();
        public Dictionary<string, int> ResearchesByMonth { get; set; } = new();
        public double AverageReviewTime { get; set; }
        public List<ReviewerPerformanceDto> ReviewerPerformance { get; set; } = new();
        public List<MonthlyStatsDto> MonthlyStatistics { get; set; } = new();
    }

    public class ReviewerPerformanceDto
    {
        public string ReviewerId { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;
        public int TotalReviews { get; set; }
        public int CompletedReviews { get; set; }
        public int PendingReviews { get; set; }
        public int OverdueReviews { get; set; }
        public double AverageReviewTime { get; set; }
        public double AverageScore { get; set; }
        public double CompletionRate => TotalReviews > 0 ?
            (double)CompletedReviews / TotalReviews * 100 : 0;
    }

    public class ReviewAssignmentDto
    {
        public int ReviewId { get; set; }
        public string ReviewerId { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;
        public DateTime AssignedDate { get; set; }
        public DateTime Deadline { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
        public ReviewDecision? Decision { get; set; }
        public decimal? Score { get; set; }
        public bool IsOverdue => !IsCompleted && Deadline < DateTime.UtcNow;
        public int DaysRemaining => IsCompleted ? 0 :
            Math.Max(0, (Deadline - DateTime.UtcNow).Days);
    }

    public class MonthlyStatsDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int SubmittedCount { get; set; }
        public int CompletedCount { get; set; }
        public int AcceptedCount { get; set; }
        public int RejectedCount { get; set; }
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
    }

    public enum BulkActionType
    {
        [Display(Name = "تحديث الحالة")]
        UpdateStatus = 1,

        [Display(Name = "تعيين مراجع")]
        AssignReviewer = 2,

        [Display(Name = "تحديد موعد نهائي")]
        SetDeadline = 3,

        [Display(Name = "إرسال تذكير")]
        SendReminder = 4,

        [Display(Name = "إنشاء تقرير")]
        GenerateReport = 5
    }
}

// Review Queries and Commands for Mediator
namespace ResearchManagement.Application.Queries.Review
{
    public class GetReviewListQuery : IRequest<PagedList<ReviewDto>>
    {
        public string UserId { get; set; }
        public UserRole UserRole { get; set; }
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public string? Track { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public GetReviewListQuery(string userId, UserRole userRole)
        {
            UserId = userId;
            UserRole = userRole;
        }
    }

    public class GetReviewByIdQuery : IRequest<ReviewDto?>
    {
        public int ReviewId { get; set; }
        public string UserId { get; set; }

        public GetReviewByIdQuery(int reviewId, string userId)
        {
            ReviewId = reviewId;
            UserId = userId;
        }
    }

    public class GetResearchFileByIdQuery : IRequest<ResearchFileDto?>
    {
        public int FileId { get; set; }
        public string UserId { get; set; }

        public GetResearchFileByIdQuery(int fileId, string userId)
        {
            FileId = fileId;
            UserId = userId;
        }
    }
}

namespace ResearchManagement.Application.Commands.Review
{
    public class UpdateReviewCommand : IRequest<bool>
    {
        public UpdateReviewDto Review { get; set; } = new();
        public string UserId { get; set; } = string.Empty;
    }

    public class CompleteReviewCommand : IRequest<bool>
    {
        public int ReviewId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public class AssignReviewerCommand : IRequest<bool>
    {
        public int ResearchId { get; set; }
        public string ReviewerId { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
        public string? Notes { get; set; }
        public string AssignedBy { get; set; } = string.Empty;
    }

    public class RemoveReviewerCommand : IRequest<bool>
    {
        public int ReviewId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public class BulkUpdateResearchStatusCommand : IRequest<bool>
    {
        public List<int> ResearchIds { get; set; } = new();
        public ResearchStatus NewStatus { get; set; }
        public string? Notes { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}