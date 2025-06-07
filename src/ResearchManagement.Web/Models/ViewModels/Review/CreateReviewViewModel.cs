using ResearchManagement.Application.DTOs;
using ResearchManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ResearchManagement.Web.Models.ViewModels.Review
{
    public class CreateReviewViewModel
    {
        public int ResearchId { get; set; }
        public string ReviewerId { get; set; } = string.Empty;
        public ResearchDto Research { get; set; } = new();

        // Review Criteria Scores (1-10)
        [Required(ErrorMessage = "تقييم الأصالة مطلوب")]
        [Range(1, 10, ErrorMessage = "التقييم يجب أن يكون بين 1 و 10")]
        [Display(Name = "تقييم الأصالة")]
        public int OriginalityScore { get; set; }

        [Required(ErrorMessage = "تقييم المنهجية مطلوب")]
        [Range(1, 10, ErrorMessage = "التقييم يجب أن يكون بين 1 و 10")]
        [Display(Name = "تقييم المنهجية")]
        public int MethodologyScore { get; set; }

        [Required(ErrorMessage = "تقييم النتائج مطلوب")]
        [Range(1, 10, ErrorMessage = "التقييم يجب أن يكون بين 1 و 10")]
        [Display(Name = "تقييم النتائج")]
        public int ResultsScore { get; set; }

        [Required(ErrorMessage = "تقييم جودة الكتابة مطلوب")]
        [Range(1, 10, ErrorMessage = "التقييم يجب أن يكون بين 1 و 10")]
        [Display(Name = "تقييم جودة الكتابة")]
        public int WritingScore { get; set; }

        // Comments for each criteria
        [StringLength(1000, ErrorMessage = "التعليقات يجب أن تكون أقل من 1000 حرف")]
        [Display(Name = "تعليقات الأصالة")]
        public string? OriginalityComments { get; set; }

        [StringLength(1000, ErrorMessage = "التعليقات يجب أن تكون أقل من 1000 حرف")]
        [Display(Name = "تعليقات المنهجية")]
        public string? MethodologyComments { get; set; }

        [StringLength(1000, ErrorMessage = "التعليقات يجب أن تكون أقل من 1000 حرف")]
        [Display(Name = "تعليقات النتائج")]
        public string? ResultsComments { get; set; }

        [StringLength(1000, ErrorMessage = "التعليقات يجب أن تكون أقل من 1000 حرف")]
        [Display(Name = "تعليقات جودة الكتابة")]
        public string? WritingComments { get; set; }

        // Overall Comments
        [Required(ErrorMessage = "تعليقات للمؤلف مطلوبة")]
        [StringLength(2000, ErrorMessage = "التعليقات يجب أن تكون أقل من 2000 حرف")]
        [Display(Name = "تعليقات للمؤلف")]
        public string CommentsToAuthor { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "التعليقات يجب أن تكون أقل من 2000 حرف")]
        [Display(Name = "تعليقات لمدير المسار")]
        public string? CommentsToTrackManager { get; set; }

        [StringLength(1000, ErrorMessage = "التوصيات يجب أن تكون أقل من 1000 حرف")]
        [Display(Name = "التوصيات")]
        public string? Recommendations { get; set; }

        // Decision
        [Required(ErrorMessage = "القرار النهائي مطلوب")]
        [Display(Name = "القرار النهائي")]
        public ReviewDecision Decision { get; set; }

        // Additional Properties
        public bool IsDraft { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Notes { get; set; }

        // Calculated Properties
        public double OverallScore => (OriginalityScore + MethodologyScore + ResultsScore + WritingScore) / 4.0;

        public string OverallScoreDisplayName => OverallScore switch
        {
            >= 9 => "ممتاز",
            >= 8 => "جيد جداً",
            >= 7 => "جيد",
            >= 6 => "مقبول",
            >= 5 => "ضعيف",
            _ => "ضعيف جداً"
        };

        public string DecisionDisplayName => Decision switch
        {
            ReviewDecision.AcceptAsIs => "قبول البحث",
            ReviewDecision.Reject => "رفض البحث",
            ReviewDecision.AcceptWithMinorRevisions => "قبول مع تعديلات طفيفة",
            ReviewDecision.MajorRevisionsRequired => "قبول مع تعديلات جوهرية",
            _ => "غير محدد"
        };

        // Validation Methods
        public bool IsValid()
        {
            return OriginalityScore >= 1 && OriginalityScore <= 10 &&
                   MethodologyScore >= 1 && MethodologyScore <= 10 &&
                   ResultsScore >= 1 && ResultsScore <= 10 &&
                   WritingScore >= 1 && WritingScore <= 10 &&
                   !string.IsNullOrWhiteSpace(CommentsToAuthor) &&
                   Decision != 0;
        }

        public List<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (OriginalityScore < 1 || OriginalityScore > 10)
                errors.Add("تقييم الأصالة يجب أن يكون بين 1 و 10");

            if (MethodologyScore < 1 || MethodologyScore > 10)
                errors.Add("تقييم المنهجية يجب أن يكون بين 1 و 10");

            if (ResultsScore < 1 || ResultsScore > 10)
                errors.Add("تقييم النتائج يجب أن يكون بين 1 و 10");

            if (WritingScore < 1 || WritingScore > 10)
                errors.Add("تقييم جودة الكتابة يجب أن يكون بين 1 و 10");

            if (string.IsNullOrWhiteSpace(CommentsToAuthor))
                errors.Add("تعليقات للمؤلف مطلوبة");

            if (Decision == 0)
                errors.Add("القرار النهائي مطلوب");

            return errors;
        }

        // Helper Methods
        public string GetScoreColor(int score)
        {
            return score switch
            {
                >= 8 => "success",
                >= 6 => "warning",
                >= 4 => "info",
                _ => "danger"
            };
        }

        public string GetDecisionColor()
        {
            return Decision switch
            {
                ReviewDecision.AcceptAsIs => "success",
                ReviewDecision.Reject => "danger",
                ReviewDecision.AcceptWithMinorRevisions => "info",
                ReviewDecision.MajorRevisionsRequired => "warning",
                _ => "secondary"
            };
        }

        public bool ShouldRecommendAcceptance()
        {
            return OverallScore >= 7 && (Decision == ReviewDecision.AcceptAsIs || Decision == ReviewDecision.AcceptWithMinorRevisions);
        }

        public bool ShouldRecommendRejection()
        {
            return OverallScore < 5 || Decision == ReviewDecision.Reject;
        }

        public int GetDaysToComplete()
        {
            if (!DueDate.HasValue) return 0;
            return Math.Max(0, (DueDate.Value - DateTime.UtcNow).Days);
        }

        public bool IsOverdue()
        {
            return DueDate.HasValue && DateTime.UtcNow > DueDate.Value;
        }
    }

    public class ReviewDetailsViewModel
    {
        public ReviewDto Review { get; set; } = new();
        public ResearchDto Research { get; set; } = new();
        public string ReviewerName { get; set; } = string.Empty;
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanView { get; set; }
        public string CurrentUserId { get; set; } = string.Empty;
        public UserRole CurrentUserRole { get; set; }

        // Review Analysis
        public List<ReviewCriteriaScore> CriteriaScores { get; set; } = new();
        public double OverallScore { get; set; }
        public string OverallScoreDisplayName { get; set; } = string.Empty;
        public TimeSpan ReviewDuration { get; set; }
        public bool IsLateSubmission { get; set; }

        // Related Reviews (for comparison)
        public List<ReviewSummaryDto> OtherReviews { get; set; } = new();
        public double AverageScore { get; set; }
        public int TotalReviews { get; set; }
    }

    public class ReviewCriteriaScore
    {
        public string CriteriaName { get; set; } = string.Empty;
        public int Score { get; set; }
        public string? Comments { get; set; }
        public double Weight { get; set; } = 0.25; // 25% each by default
        public string ScoreColor => Score switch
        {
            >= 8 => "success",
            >= 6 => "warning",
            >= 4 => "info",
            _ => "danger"
        };
    }

    public class ReviewSummaryDto
    {
        public int Id { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public double OverallScore { get; set; }
        public ReviewDecision Decision { get; set; }
        public DateTime CompletedDate { get; set; }
        public bool IsCurrentUserReview { get; set; }
    }
}