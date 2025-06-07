using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;
namespace ResearchManagement.Web.Models.ViewModels
{
    public class ReviewerDashboardViewModel
    {
        public int TotalReviews { get; set; }
        public int CompletedReviews { get; set; }
        public int PendingReviews { get; set; }
        public int OverdueReviews { get; set; }
        public List<Domain.Entities.Review> RecentReviews { get; set; } = new();
    }

    public class ReviewFilterModel
    {
        public bool? IsCompleted { get; set; }
        public ReviewDecision? Decision { get; set; }
        public ResearchTrack? Track { get; set; }
        public DateTime? DeadlineFrom { get; set; }
        public DateTime? DeadlineTo { get; set; }
        public bool ShowOverdueOnly { get; set; }
    }

    public class ReviewerStatistics
    {
        public int TotalAssigned { get; set; }
        public int Completed { get; set; }
        public int Pending { get; set; }
        public int Overdue { get; set; }
        public decimal AverageCompletionTime { get; set; } // بالأيام
        public decimal AverageScore { get; set; }
    }
}
