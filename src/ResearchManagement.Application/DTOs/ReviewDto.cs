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
        public string ReviewerId { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;
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
        public bool IsCompleted { get; set; }
        public bool RequiresReReview { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

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
        public decimal OverallScore { get; set; }
        public ReviewDecision Decision { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsCurrentUserReview { get; set; }
        public string StatusDisplayName => IsCompleted ? "مكتملة" : "معلقة";
    }
}



//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ResearchManagement.Domain.Enums;

//namespace ResearchManagement.Application.DTOs
//{
//    public class ReviewDto
//    {
//        public int Id { get; set; }
//        public ReviewDecision Decision { get; set; }
//        public int OriginalityScore { get; set; }
//        public int MethodologyScore { get; set; }
//        public int ClarityScore { get; set; }
//        public int SignificanceScore { get; set; }
//        public int ReferencesScore { get; set; }
//        public decimal OverallScore { get; set; }
//        public decimal Score => OverallScore; // Alias for compatibility
//        public string? CommentsToAuthor { get; set; }
//        public string? CommentsToTrackManager { get; set; }
//        public DateTime AssignedDate { get; set; }
//        public DateTime CreatedAt { get; set; }
//        public DateTime? CompletedDate { get; set; }
//        public DateTime Deadline { get; set; }
//        public bool IsCompleted { get; set; }
//        public bool RequiresReReview { get; set; }
//        public string ReviewerId { get; set; } = string.Empty;
//        public string ReviewerName { get; set; } = string.Empty;
//        public int ResearchId { get; set; }
//        public string ResearchTitle { get; set; } = string.Empty;
//        public List<ResearchFileDto> ReviewFiles { get; set; } = new();
//    }

//    public class CreateReviewDto
//    {
//        public ReviewDecision Decision { get; set; }
//        public int OriginalityScore { get; set; }
//        public int MethodologyScore { get; set; }
//        public int ClarityScore { get; set; }
//        public int SignificanceScore { get; set; }
//        public int ReferencesScore { get; set; }
//        public string? CommentsToAuthor { get; set; }
//        public string? CommentsToTrackManager { get; set; }
//        public bool RequiresReReview { get; set; } = false;
//        public int ResearchId { get; set; }
//    }

//    public class UpdateReviewDto : CreateReviewDto
//    {
//        public int Id { get; set; }
//    }
//}
