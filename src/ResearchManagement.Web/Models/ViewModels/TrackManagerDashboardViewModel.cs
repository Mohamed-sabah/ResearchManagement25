using ResearchManagement.Domain.Entities;

namespace ResearchManagement.Web.Models.ViewModels
{
    public class TrackManagerDashboardViewModel
    {
        public int TotalResearches { get; set; }
        public int TotalReviewers { get; set; }
        public int PendingAssignments { get; set; }
        public int CompletedReviews { get; set; }
        public string TrackName { get; set; } = string.Empty;
        public List<Domain.Entities.Research> RecentResearches { get; set; } = new();
        public List<Domain.Entities.Review> OverdueReviews { get; set; } = new();
    }
}
