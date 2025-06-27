using ResearchManagement.Application.DTOs;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Web.Models.ViewModels.TrackManager
{
    public class TrackManagerDashboardViewModel
    {
        public string TrackName { get; set; } = string.Empty;
        public ResearchTrack Track { get; set; }
        public TrackStatisticsDto Statistics { get; set; } = new();
        public List<Research> RecentResearches { get; set; } = new();
        public List<Research> PendingResearches { get; set; } = new();
        public List<Research> ResearchesNeedingReviewers { get; set; } = new();
        public List<Review> OverdueReviews { get; set; } = new();
    }

    public class TrackResearchesViewModel
    {
        public List<Research> Researches { get; set; } = new();
        public string TrackName { get; set; } = string.Empty;
        public ResearchTrack Track { get; set; }
        public ResearchFilterDto Filter { get; set; } = new();
        public PaginationDto Pagination { get; set; } = new();
    }

    public class AssignReviewersViewModel
    {
        public Research Research { get; set; } = new();
        public List<User> AvailableReviewers { get; set; } = new();
        public List<ReviewDto> CurrentReviews { get; set; } = new();
        public string TrackName { get; set; } = string.Empty;
    }

    public class ReviewerManagementViewModel
    {
        public string TrackName { get; set; } = string.Empty;
        public ResearchTrack Track { get; set; }
        public List<ReviewerStatisticsDto> ReviewerStatistics { get; set; } = new();
    }

    public class TrackReportsViewModel
    {
        public TrackReportDto Report { get; set; } = new();
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}