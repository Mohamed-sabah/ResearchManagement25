using ResearchManagement.Application.DTOs;
using ResearchManagement.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ResearchManagement.Web.Models.ViewModels.Review
{
    public class ReviewListViewModel
    {
        public PagedResult<ReviewDto> Reviews { get; set; } = new();
        public ReviewStatisticsDto Statistics { get; set; } = new();
        public ReviewFilterViewModel Filter { get; set; } = new();
        public List<SelectListItem> TrackOptions { get; set; } = new();
        public string CurrentUserId { get; set; } = string.Empty;
        public UserRole CurrentUserRole { get; set; }
        public bool CanCreateReview { get; set; }
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
    }

    public class ReviewDetailsViewModel
    {
        public ReviewDto Review { get; set; } = new();
        public string CurrentUserId { get; set; } = string.Empty;
        public UserRole CurrentUserRole { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool IsReviewer { get; set; }
    }

    public class CreateReviewViewModel
    {
        public int ResearchId { get; set; }
        public string ReviewerId { get; set; } = string.Empty;
        public ResearchSummaryDto Research { get; set; } = new();

        [Required(ErrorMessage = "يجب اختيار القرار النهائي")]
        public ReviewDecision Decision { get; set; }

        [Range(1, 10, ErrorMessage = "يجب أن تكون النقاط بين 1 و 10")]
        [Required(ErrorMessage = "نقاط الأصالة مطلوبة")]
        public int OriginalityScore { get; set; }

        [Range(1, 10, ErrorMessage = "يجب أن تكون النقاط بين 1 و 10")]
        [Required(ErrorMessage = "نقاط المنهجية مطلوبة")]
        public int MethodologyScore { get; set; }

        [Range(1, 10, ErrorMessage = "يجب أن تكون النقاط بين 1 و 10")]
        [Required(ErrorMessage = "نقاط الوضوح مطلوبة")]
        public int ClarityScore { get; set; }

        [Range(1, 10, ErrorMessage = "يجب أن تكون النقاط بين 1 و 10")]
        [Required(ErrorMessage = "نقاط الأهمية مطلوبة")]
        public int SignificanceScore { get; set; }

        [Range(1, 10, ErrorMessage = "يجب أن تكون النقاط بين 1 و 10")]
        [Required(ErrorMessage = "نقاط المراجع مطلوبة")]
        public int ReferencesScore { get; set; }

        [Required(ErrorMessage = "التعليقات للمؤلف مطلوبة")]
        [StringLength(2000, ErrorMessage = "التعليقات يجب ألا تزيد عن 2000 حرف")]
        public string CommentsToAuthor { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "التعليقات يجب ألا تزيد عن 2000 حرف")]
        public string? CommentsToTrackManager { get; set; }

        [StringLength(1000, ErrorMessage = "التوصيات يجب ألا تزيد عن 1000 حرف")]
        public string? Recommendations { get; set; }

        public DateTime? Deadline { get; set; }
        public bool RequiresReReview { get; set; }

        // Additional comment fields for better organization
        public string? OriginalityComments { get; set; }
        public string? MethodologyComments { get; set; }
        public string? WritingComments { get; set; }
        public string? ResultsComments { get; set; }

        // Calculated property
        public decimal OverallScore => (OriginalityScore * 0.2m + MethodologyScore * 0.25m +
                                      ClarityScore * 0.2m + SignificanceScore * 0.2m +
                                      ReferencesScore * 0.15m);
    }

    public class EditReviewViewModel : CreateReviewViewModel
    {
        public int Id { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime? AssignedDate { get; set; }
    }

    public class PendingReviewsViewModel
    {
        public List<ReviewDto> Reviews { get; set; } = new();
        public string CurrentUserId { get; set; } = string.Empty;
    }

    public class ReviewHistoryViewModel
    {
        public List<ReviewDto> Reviews { get; set; } = new();
        public ReviewerStatisticsDto Statistics { get; set; } = new();
        public string CurrentUserId { get; set; } = string.Empty;
    }
}