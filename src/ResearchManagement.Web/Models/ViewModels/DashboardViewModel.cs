
using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Web.Models.ViewModels
{
    public class DashboardViewModel
    {
        public UserRole UserRole { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DashboardStatistics Statistics { get; set; } = new();
        public List<Research> RecentItems { get; set; } = new();
        public List<NotificationItem> Notifications { get; set; } = new();
    }

    public class DashboardStatistics
    {
        public int TotalResearches { get; set; }
        public int AcceptedResearches { get; set; }
        public int PendingResearches { get; set; }
        public int RejectedResearches { get; set; }
        public int CompletedReviews { get; set; }
        public int PendingReviews { get; set; }
        public int OverdueReviews { get; set; }
        public decimal AverageScore { get; set; }
    }

    public class NotificationItem
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // info, warning, success, danger
        public DateTime CreatedAt { get; set; }
        public string? Url { get; set; }
        public bool IsRead { get; set; }
    }
}
