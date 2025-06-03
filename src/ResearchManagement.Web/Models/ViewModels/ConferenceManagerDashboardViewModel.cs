using ResearchManagement.Domain.Entities;

namespace ResearchManagement.Web.Models.ViewModels
{
    public class ConferenceManagerDashboardViewModel
    {
        // إحصائيات البحوث
        public int TotalResearches { get; set; }
        public int AcceptedResearches { get; set; }
        public int PendingResearches { get; set; }
        public int RejectedResearches { get; set; }
        public int SubmittedResearches { get; set; }

        // إحصائيات المستخدمين
        public int TotalUsers { get; set; }
        public int TotalReviewers { get; set; }

        // إحصائيات المراجعات
        public int CompletedReviews { get; set; }
        public double AverageReviewTime { get; set; }

        // البيانات الحديثة
        public IEnumerable<Research> RecentSubmissions { get; set; } = new List<Research>();
        public IEnumerable<User> RecentUsers { get; set; } = new List<User>();

        // إحصائيات التخصصات
        public IEnumerable<TrackStatistic> TrackStatistics { get; set; } = new List<TrackStatistic>();
    }

    public class TrackStatistic
    {
        public string TrackName { get; set; } = string.Empty;
        public int ResearchCount { get; set; }
        public int AcceptedCount { get; set; }
        public int RejectedCount { get; set; }
        public int PendingCount { get; set; }
    }
}
