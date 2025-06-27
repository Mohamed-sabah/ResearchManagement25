using ResearchManagement.Domain.Enums;
using System;
using System.Collections.Generic;



namespace ResearchManagement.Application.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int ResearchId { get; set; }
        public string ResearchTitle { get; set; } = string.Empty;
        public string? ResearchTitleEn { get; set; }
        public string ReviewerId { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;
        public string ReviewerEmail { get; set; } = string.Empty;
        public ReviewDecision Decision { get; set; }
        public int OriginalityScore { get; set; }
        public int MethodologyScore { get; set; }
        public int ClarityScore { get; set; }
        public int SignificanceScore { get; set; }
        public int ReferencesScore { get; set; }
        public decimal OverallScore { get; set; }
        public decimal Score => OverallScore; // للتوافق مع الكود الموجود
        public string? CommentsToAuthor { get; set; }
        public string? CommentsToTrackManager { get; set; }
        public string? Recommendations { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime? DueDate => Deadline; // للتوافق
        public bool IsCompleted { get; set; }
        public bool RequiresReReview { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        // Research related info
        public ResearchTrack Track { get; set; }
        public string ResearchAuthor { get; set; } = string.Empty;
        public List<ResearchAuthorDto> Authors { get; set; } = new();
        public List<ResearchFileDto> Files { get; set; } = new();

        // Additional properties for UI
        public string DecisionDisplayName => Decision switch
        {
            ReviewDecision.AcceptAsIs => "قبول فوري",
            ReviewDecision.AcceptWithMinorRevisions => "قبول مع تعديلات طفيفة",
            ReviewDecision.MajorRevisionsRequired => "تعديلات جوهرية مطلوبة",
            ReviewDecision.Reject => "رفض",
            ReviewDecision.NotSuitableForConference => "غير مناسب للمؤتمر",
            ReviewDecision.NotReviewed => "لم يتم المراجعة بعد",
            _ => "غير محدد"
        };

        public string TrackDisplayName => Track switch
        {
            ResearchTrack.InformationTechnology => "تقنية المعلومات",
            ResearchTrack.InformationSecurity => "أمن المعلومات",
            ResearchTrack.SoftwareEngineering => "هندسة البرمجيات",
            ResearchTrack.ArtificialIntelligence => "الذكاء الاصطناعي",
            ResearchTrack.DataScience => "علوم البيانات",
            ResearchTrack.NetworkingAndCommunications => "الشبكات والاتصالات",
            _ => Track.ToString()
        };

        public string StatusDisplayName => IsCompleted ? "مكتملة" : "معلقة";

        public bool IsOverdue => !IsCompleted && DateTime.UtcNow > Deadline;

        public int DaysRemaining => IsCompleted ? 0 : Math.Max(0, (Deadline - DateTime.UtcNow).Days);

        public string UrgencyLevel
        {
            get
            {
                if (IsCompleted) return "مكتملة";
                if (IsOverdue) return "متأخرة";
                if (DaysRemaining <= 1) return "عاجلة";
                if (DaysRemaining <= 3) return "مهمة";
                return "عادية";
            }
        }

        public string UrgencyBadgeClass
        {
            get
            {
                if (IsCompleted) return "badge-success";
                if (IsOverdue) return "badge-danger";
                if (DaysRemaining <= 1) return "badge-danger";
                if (DaysRemaining <= 3) return "badge-warning";
                return "badge-info";
            }
        }
    }

    public class CreateReviewDto
    {
        public int ResearchId { get; set; }
        public ReviewDecision Decision { get; set; }
        public int OriginalityScore { get; set; }
        public int MethodologyScore { get; set; }
        public int ClarityScore { get; set; }
        public int SignificanceScore { get; set; }
        public int ReferencesScore { get; set; }
        public string? CommentsToAuthor { get; set; }
        public string? CommentsToTrackManager { get; set; }
        public string? Recommendations { get; set; }
        public DateTime? Deadline { get; set; }
        public bool RequiresReReview { get; set; } = false;

        // Calculated property
        public decimal OverallScore => (OriginalityScore * 0.2m + MethodologyScore * 0.25m +
                                      ClarityScore * 0.2m + SignificanceScore * 0.2m +
                                      ReferencesScore * 0.15m);
    }

    public class UpdateReviewDto : CreateReviewDto
    {
        public int Id { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? ReviewerId { get; set; }
        public DateTime? AssignedDate { get; set; }
    }

    public class ReviewSummaryDto
    {
        public int Id { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public string ReviewerEmail { get; set; } = string.Empty;
        public decimal OverallScore { get; set; }
        public ReviewDecision Decision { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsCurrentUserReview { get; set; }
        public string StatusDisplayName => IsCompleted ? "مكتملة" : "معلقة";
        public string DecisionDisplayName => Decision switch
        {
            ReviewDecision.AcceptAsIs => "قبول فوري",
            ReviewDecision.AcceptWithMinorRevisions => "قبول مع تعديلات طفيفة",
            ReviewDecision.MajorRevisionsRequired => "تعديلات جوهرية مطلوبة",
            ReviewDecision.Reject => "رفض",
            ReviewDecision.NotSuitableForConference => "غير مناسب للمؤتمر",
            ReviewDecision.NotReviewed => "لم يتم المراجعة بعد",
            _ => "غير محدد"
        };
    }

    public class ReviewStatisticsDto
    {
        public int TotalReviews { get; set; }
        public int CompletedReviews { get; set; }
        public int PendingReviews { get; set; }
        public int OverdueReviews { get; set; }
        public double AverageScore { get; set; }
        public double AverageReviewTime { get; set; } // في الأيام
        public double AcceptanceRate { get; set; } // نسبة مئوية
    }

    public class ReviewerDto
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public string? Institution { get; set; }
        public string? AcademicDegree { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ReviewerStatisticsDto
    {
        public string ReviewerId { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;
        public string ReviewerEmail { get; set; } = string.Empty;
        public int TotalAssigned { get; set; }
        public int CompletedReviews { get; set; }
        public int PendingReviews { get; set; }
        public int OverdueReviews { get; set; }
        public double AverageReviewTime { get; set; } // في الأيام
        public double AverageScore { get; set; }
        public double AcceptanceRate { get; set; } // نسبة مئوية
        public bool IsActive { get; set; }
        public DateTime? LastReviewDate { get; set; }

        // Computed properties
        public double CompletionRate => TotalAssigned > 0 ? (double)CompletedReviews / TotalAssigned * 100 : 0;
        public string PerformanceLevel
        {
            get
            {
                if (CompletionRate >= 90 && AverageReviewTime <= 7) return "ممتاز";
                if (CompletionRate >= 80 && AverageReviewTime <= 10) return "جيد جداً";
                if (CompletionRate >= 70 && AverageReviewTime <= 14) return "جيد";
                if (CompletionRate >= 60) return "مقبول";
                return "يحتاج تحسين";
            }
        }
    }
}
