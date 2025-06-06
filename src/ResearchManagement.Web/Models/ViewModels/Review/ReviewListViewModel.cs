using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Queries.Research;
using ResearchManagement.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ResearchManagement.Web.Models.ViewModels.Review
{
    public class ReviewListViewModel
    {
        public PagedResult<ReviewItemViewModel> Reviews { get; set; } = new();
        public ReviewFilterViewModel Filter { get; set; } = new();
        public ReviewStatisticsViewModel Statistics { get; set; } = new();
        public List<SelectListItem> TrackOptions { get; set; } = new();
        public string CurrentUserId { get; set; } = string.Empty;
        public UserRole CurrentUserRole { get; set; }
        public bool CanCreateReview { get; set; }
        public bool CanManageReviews { get; set; }
    }

    public class ReviewFilterViewModel
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public string? Track { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "AssignedDate";
        public bool SortDescending { get; set; } = true;
    }

    public class ReviewItemViewModel
    {
        public int Id { get; set; }
        public int ResearchId { get; set; }
        public string ResearchTitle { get; set; } = string.Empty;
        public string? ResearchTitleEn { get; set; }
        public string ResearchAuthor { get; set; } = string.Empty;
        public ResearchTrack Track { get; set; }
        public string ReviewerId { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;
        public DateTime AssignedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsOverdue => DueDate.HasValue && !IsCompleted && DateTime.UtcNow > DueDate.Value;
        public int? Score { get; set; }
        public ReviewDecision? Decision { get; set; }
        public string? CommentsToAuthor { get; set; }
        public string? CommentsToTrackManager { get; set; }

        public string TrackDisplayName => Track switch
        {
            ResearchTrack.ComputerScience => "علوم الحاسوب",
            ResearchTrack.InformationSystems => "نظم المعلومات",
            ResearchTrack.SoftwareEngineering => "هندسة البرمجيات",
            ResearchTrack.ArtificialIntelligence => "الذكاء الاصطناعي",
            ResearchTrack.DataScience => "علوم البيانات",
            ResearchTrack.Cybersecurity => "الأمن السيبراني",
            ResearchTrack.NetworkSystems => "أنظمة الشبكات",
            ResearchTrack.HumanComputerInteraction => "التفاعل بين الإنسان والحاسوب",
            _ => "غير محدد"
        };

        public string StatusDisplayName
        {
            get
            {
                if (IsCompleted) return "مكتملة";
                if (IsOverdue) return "متأخرة";
                return "معلقة";
            }
        }

        public string StatusBadgeClass
        {
            get
            {
                if (IsCompleted) return "badge-success";
                if (IsOverdue) return "badge-danger";
                return "badge-warning";
            }
        }

        public string DecisionDisplayName => Decision switch
        {
            ReviewDecision.Accept => "قبول",
            ReviewDecision.Reject => "رفض",
            ReviewDecision.MinorRevisions => "تعديلات طفيفة",
            ReviewDecision.MajorRevisions => "تعديلات جوهرية",
            _ => "غير محدد"
        };

        public string DecisionBadgeClass => Decision switch
        {
            ReviewDecision.Accept => "badge-success",
            ReviewDecision.Reject => "badge-danger",
            ReviewDecision.MinorRevisions => "badge-info",
            ReviewDecision.MajorRevisions => "badge-warning",
            _ => "badge-secondary"
        };

        public int DaysRemaining
        {
            get
            {
                if (!DueDate.HasValue || IsCompleted) return 0;
                return Math.Max(0, (DueDate.Value - DateTime.UtcNow).Days);
            }
        }

        public int DaysOverdue
        {
            get
            {
                if (!IsOverdue) return 0;
                return (DateTime.UtcNow - DueDate!.Value).Days;
            }
        }
    }

    public class ReviewStatisticsViewModel
    {
        public int TotalReviews { get; set; }
        public int PendingReviews { get; set; }
        public int CompletedReviews { get; set; }
        public int OverdueReviews { get; set; }
        public double AverageScore { get; set; }
        public double CompletionRate => TotalReviews > 0 ? (double)CompletedReviews / TotalReviews * 100 : 0;
        public double OverdueRate => TotalReviews > 0 ? (double)OverdueReviews / TotalReviews * 100 : 0;

        // Distribution by decision
        public int AcceptedCount { get; set; }
        public int RejectedCount { get; set; }
        public int MinorRevisionsCount { get; set; }
        public int MajorRevisionsCount { get; set; }

        // Time statistics
        public double AverageReviewTime { get; set; } // in days
        public int FastestReviewTime { get; set; } // in days
        public int SlowestReviewTime { get; set; } // in days

        // Monthly statistics
        public List<MonthlyReviewStats> MonthlyStats { get; set; } = new();
    }

    public class MonthlyReviewStats
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int CompletedReviews { get; set; }
        public double AverageScore { get; set; }
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
    }
}