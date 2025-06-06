using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Web.Models.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public UserRole UserRole { get; set; }
        public string RoleDisplayName { get; set; } = string.Empty;
        public DateTime? LastLoginDate { get; set; }
        public bool CanCreateResearch { get; set; }
        public bool CanManageResearches { get; set; }
        public bool CanReview { get; set; }

        // Statistics
        public DashboardStatistics Statistics { get; set; } = new();

        // Recent Activities
        public List<ActivityViewModel> RecentActivities { get; set; } = new();

        // Notifications
        public List<NotificationViewModel> Notifications { get; set; } = new();

        // Charts Data (for Admin/TrackManager)
        public List<StatusDistributionItem> StatusDistribution { get; set; } = new();
        public List<MonthlySubmissionItem> MonthlySubmissions { get; set; } = new();
        public List<TrackPerformanceItem> TrackPerformance { get; set; } = new();

        // Quick Links
        public List<QuickLinkViewModel> QuickLinks { get; set; } = new();

        // Upcoming Deadlines
        public List<DeadlineViewModel> UpcomingDeadlines { get; set; } = new();
    }

    public class DashboardStatistics
    {
        public int TotalResearches { get; set; }
        public int PendingReviews { get; set; }
        public int CompletedReviews { get; set; }
        public int OverdueReviews { get; set; }
        public double AverageScore { get; set; }
        public double CompletionRate { get; set; }
        public double ResearchGrowthRate { get; set; } // Percentage change from last month
        public int AcceptedResearches { get; set; }
        public int RejectedResearches { get; set; }
        public int PublishedResearches { get; set; }

        // User-specific stats
        public int MyResearches { get; set; }
        public int MyPendingReviews { get; set; }
        public int MyCompletedReviews { get; set; }
        public double MyAverageScore { get; set; }

        // Time-based stats
        public int ThisMonthSubmissions { get; set; }
        public int LastMonthSubmissions { get; set; }
        public int ThisYearSubmissions { get; set; }
        public int LastYearSubmissions { get; set; }
    }

    public class ActivityViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ActivityType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ActionUrl { get; set; }
        public string? UserName { get; set; }
        public string? EntityName { get; set; }
        public int? EntityId { get; set; }

        public string TypeColor => Type switch
        {
            ActivityType.ResearchSubmitted => "primary",
            ActivityType.ResearchAccepted => "success",
            ActivityType.ResearchRejected => "danger",
            ActivityType.ReviewCompleted => "info",
            ActivityType.StatusChanged => "warning",
            ActivityType.UserRegistered => "secondary",
            ActivityType.FileUploaded => "info",
            _ => "secondary"
        };

        public string TypeIcon => Type switch
        {
            ActivityType.ResearchSubmitted => "file-plus",
            ActivityType.ResearchAccepted => "check-circle",
            ActivityType.ResearchRejected => "times-circle",
            ActivityType.ReviewCompleted => "clipboard-check",
            ActivityType.StatusChanged => "exchange-alt",
            ActivityType.UserRegistered => "user-plus",
            ActivityType.FileUploaded => "upload",
            _ => "info-circle"
        };

        public string RelativeTime
        {
            get
            {
                var timeSpan = DateTime.UtcNow - CreatedAt;
                
                if (timeSpan.TotalMinutes < 1)
                    return "الآن";
                if (timeSpan.TotalMinutes < 60)
                    return $"منذ {(int)timeSpan.TotalMinutes} دقيقة";
                if (timeSpan.TotalHours < 24)
                    return $"منذ {(int)timeSpan.TotalHours} ساعة";
                if (timeSpan.TotalDays < 7)
                    return $"منذ {(int)timeSpan.TotalDays} يوم";
                if (timeSpan.TotalDays < 30)
                    return $"منذ {(int)(timeSpan.TotalDays / 7)} أسبوع";
                
                return CreatedAt.ToString("yyyy/MM/dd");
            }
        }
    }

    public enum ActivityType
    {
        ResearchSubmitted = 1,
        ResearchAccepted = 2,
        ResearchRejected = 3,
        ReviewCompleted = 4,
        StatusChanged = 5,
        UserRegistered = 6,
        FileUploaded = 7,
        ReviewAssigned = 8,
        DeadlineReminder = 9,
        SystemUpdate = 10
    }

    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string? ActionUrl { get; set; }
        public int? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }

        public string TypeColor => Type switch
        {
            NotificationType.Info => "info",
            NotificationType.Success => "success",
            NotificationType.Warning => "warning",
            NotificationType.Error => "danger",
            NotificationType.Reminder => "primary",
            _ => "secondary"
        };

        public string Icon => Type switch
        {
            NotificationType.Info => "info-circle",
            NotificationType.Success => "check-circle",
            NotificationType.Warning => "exclamation-triangle",
            NotificationType.Error => "times-circle",
            NotificationType.Reminder => "bell",
            _ => "info-circle"
        };

        public string RelativeTime
        {
            get
            {
                var timeSpan = DateTime.UtcNow - CreatedAt;
                
                if (timeSpan.TotalMinutes < 1)
                    return "الآن";
                if (timeSpan.TotalMinutes < 60)
                    return $"منذ {(int)timeSpan.TotalMinutes} دقيقة";
                if (timeSpan.TotalHours < 24)
                    return $"منذ {(int)timeSpan.TotalHours} ساعة";
                if (timeSpan.TotalDays < 7)
                    return $"منذ {(int)timeSpan.TotalDays} يوم";
                
                return CreatedAt.ToString("yyyy/MM/dd");
            }
        }
    }

    public enum NotificationType
    {
        Info = 1,
        Success = 2,
        Warning = 3,
        Error = 4,
        Reminder = 5
    }

    public class StatusDistributionItem
    {
        public string StatusName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class MonthlySubmissionItem
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Count { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public double GrowthRate { get; set; }
    }

    public class TrackPerformanceItem
    {
        public ResearchTrack Track { get; set; }
        public string TrackName { get; set; } = string.Empty;
        public int TotalResearches { get; set; }
        public int CompletedReviews { get; set; }
        public int PendingReviews { get; set; }
        public double AverageScore { get; set; }
        public double AcceptanceRate { get; set; }
        public int AverageReviewTime { get; set; } // in days
        public int ActiveReviewers { get; set; }
        public string? TrackManagerName { get; set; }
    }

    public class QuickLinkViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = "primary";
        public bool IsExternal { get; set; }
        public int? BadgeCount { get; set; }
        public string? BadgeColor { get; set; }
    }

    public class DeadlineViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public DeadlineType Type { get; set; }
        public string? ActionUrl { get; set; }
        public bool IsOverdue { get; set; }
        public bool IsUrgent { get; set; }

        public string TypeColor => Type switch
        {
            DeadlineType.ReviewDeadline => "warning",
            DeadlineType.SubmissionDeadline => "primary",
            DeadlineType.RevisionDeadline => "info",
            DeadlineType.PublicationDeadline => "success",
            _ => "secondary"
        };

        public string TypeIcon => Type switch
        {
            DeadlineType.ReviewDeadline => "clipboard-check",
            DeadlineType.SubmissionDeadline => "file-upload",
            DeadlineType.RevisionDeadline => "edit",
            DeadlineType.PublicationDeadline => "globe",
            _ => "calendar"
        };

        public int DaysRemaining => Math.Max(0, (DueDate - DateTime.UtcNow).Days);

        public string UrgencyLevel
        {
            get
            {
                if (IsOverdue) return "متأخر";
                if (DaysRemaining <= 1) return "عاجل";
                if (DaysRemaining <= 3) return "مهم";
                if (DaysRemaining <= 7) return "قريب";
                return "عادي";
            }
        }

        public string UrgencyColor
        {
            get
            {
                if (IsOverdue) return "danger";
                if (DaysRemaining <= 1) return "danger";
                if (DaysRemaining <= 3) return "warning";
                if (DaysRemaining <= 7) return "info";
                return "success";
            }
        }
    }

    public enum DeadlineType
    {
        ReviewDeadline = 1,
        SubmissionDeadline = 2,
        RevisionDeadline = 3,
        PublicationDeadline = 4,
        ConferenceDeadline = 5,
        SystemMaintenance = 6
    }

    // Helper classes for dashboard widgets
    public class DashboardWidget
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // chart, list, stat, etc.
        public object Data { get; set; } = new();
        public int Order { get; set; }
        public bool IsVisible { get; set; } = true;
        public string Size { get; set; } = "col-md-6"; // Bootstrap column class
        public Dictionary<string, object> Settings { get; set; } = new();
    }

    public class UserPreferences
    {
        public string UserId { get; set; } = string.Empty;
        public List<DashboardWidget> Widgets { get; set; } = new();
        public string Theme { get; set; } = "light";
        public string Language { get; set; } = "ar";
        public bool EmailNotifications { get; set; } = true;
        public bool PushNotifications { get; set; } = true;
        public int RefreshInterval { get; set; } = 300; // seconds
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}