using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;
namespace ResearchManagement.Web.Models.ViewModels
{
    public class ReviewListViewModel
    {
        public List<Review> Reviews { get; set; } = new();
        public ReviewFilterModel Filter { get; set; } = new();
        public PaginationModel Pagination { get; set; } = new();
        public string ViewTitle { get; set; } = "المراجعات";
        public ReviewerStatistics Statistics { get; set; } = new();
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
