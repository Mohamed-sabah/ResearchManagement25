//using ResearchManagement.Application.DTOs;
//using ResearchManagement.Domain.Entities;
//using ResearchManagement.Domain.Enums;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using System.ComponentModel.DataAnnotations;

//namespace ResearchManagement.Web.Models.ViewModels.Review
//{
//    // Review List ViewModel
//    public class ReviewListViewModel
//    {
//        public PagedList<ReviewDto> Reviews { get; set; } = new();
//        public ReviewStatisticsDto Statistics { get; set; } = new();
//        public ReviewFilterViewModel Filter { get; set; } = new();
//        public List<SelectListItem> TrackOptions { get; set; } = new();
//        public string CurrentUserId { get; set; } = string.Empty;
//        public UserRole CurrentUserRole { get; set; }
//        public bool CanCreateReview { get; set; }
//    }

//    // Review Filter ViewModel
//    public class ReviewFilterViewModel
//    {
//        public string? SearchTerm { get; set; }
//        public string? Status { get; set; }
//        public string? Track { get; set; }
//        public DateTime? FromDate { get; set; }
//        public DateTime? ToDate { get; set; }
//        public int Page { get; set; } = 1;
//        public int PageSize { get; set; } = 10;
//    }

//    // Review Details ViewModel
//    public class ReviewDetailsViewModel
//    {
//        public ReviewDto Review { get; set; } = new();
//        public string CurrentUserId { get; set; } = string.Empty;
//        public UserRole CurrentUserRole { get; set; }
//        public bool CanEdit { get; set; }
//        public bool CanDelete { get; set; }
//        public bool IsReviewer { get; set; }
//    }

//    // Create Review ViewModel
//    public class CreateReviewViewModel
//    {
//        public int ResearchId { get; set; }
//        public string ReviewerId { get; set; } = string.Empty;

//        [Display(Name = "البحث")]
//        public ResearchSummaryDto Research { get; set; } = new();

//        [Required(ErrorMessage = "يرجى اختيار القرار")]
//        [Display(Name = "القرار النهائي")]
//        public ReviewDecision Decision { get; set; }

//        [Required(ErrorMessage = "تقييم الأصالة مطلوب")]
//        [Range(1, 10, ErrorMessage = "التقييم يجب أن يكون بين 1 و 10")]
//        [Display(Name = "الأصالة والجدة")]
//        public int OriginalityScore { get; set; }

//        [Required(ErrorMessage = "تقييم المنهجية مطلوب")]
//        [Range(1, 10, ErrorMessage = "التقييم يجب أن يكون بين 1 و 10")]
//        [Display(Name = "المنهجية")]
//        public int MethodologyScore { get; set; }

//        [Required(ErrorMessage = "تقييم الوضوح مطلوب")]
//        [Range(1, 10, ErrorMessage = "التقييم يجب أن يكون بين 1 و 10")]
//        [Display(Name = "الوضوح والكتابة")]
//        public int ClarityScore { get; set; }

//        [Required(ErrorMessage = "تقييم الأهمية مطلوب")]
//        [Range(1, 10, ErrorMessage = "التقييم يجب أن يكون بين 1 و 10")]
//        [Display(Name = "الأهمية")]
//        public int SignificanceScore { get; set; }

//        [Required(ErrorMessage = "تقييم المراجع مطلوب")]
//        [Range(1, 10, ErrorMessage = "التقييم يجب أن يكون بين 1 و 10")]
//        [Display(Name = "المراجع")]
//        public int ReferencesScore { get; set; }

//        [Required(ErrorMessage = "التعليقات للمؤلف مطلوبة")]
//        [Display(Name = "تعليقات للمؤلف")]
//        [StringLength(2000, ErrorMessage = "التعليقات يجب ألا تتجاوز 2000 حرف")]
//        public string CommentsToAuthor { get; set; } = string.Empty;

//        [Display(Name = "تعليقات لمدير المسار")]
//        [StringLength(2000, ErrorMessage = "التعليقات يجب ألا تتجاوز 2000 حرف")]
//        public string? CommentsToTrackManager { get; set; }

//        [Display(Name = "التوصيات")]
//        [StringLength(1000, ErrorMessage = "التوصيات يجب ألا تتجاوز 1000 حرف")]
//        public string? Recommendations { get; set; }

//        [Display(Name = "تعليقات على الأصالة")]
//        [StringLength(500)]
//        public string? OriginalityComments { get; set; }

//        [Display(Name = "تعليقات على المنهجية")]
//        [StringLength(500)]
//        public string? MethodologyComments { get; set; }

//        [Display(Name = "تعليقات على النتائج")]
//        [StringLength(500)]
//        public string? ResultsComments { get; set; }

//        [Display(Name = "تعليقات على الكتابة")]
//        [StringLength(500)]
//        public string? WritingComments { get; set; }

//        [Display(Name = "يتطلب مراجعة إضافية")]
//        public bool RequiresReReview { get; set; }

//        [Display(Name = "الموعد النهائي")]
//        public DateTime? Deadline { get; set; }

//        // Calculated Properties
//        public decimal OverallScore => (OriginalityScore * 0.2m + MethodologyScore * 0.25m +
//                                      ClarityScore * 0.2m + SignificanceScore * 0.2m +
//                                      ReferencesScore * 0.15m);
//    }

//    // Edit Review ViewModel
//    public class EditReviewViewModel : CreateReviewViewModel
//    {
//        public int Id { get; set; }

//        [Display(Name = "مكتملة")]
//        public bool IsCompleted { get; set; }

//        [Display(Name = "تاريخ الإكمال")]
//        public DateTime? CompletedDate { get; set; }
//    }

//    // Pending Reviews ViewModel
//    public class PendingReviewsViewModel
//    {
//        public List<ReviewDto> Reviews { get; set; } = new();
//        public string CurrentUserId { get; set; } = string.Empty;
//        public int TotalPending => Reviews.Count;
//        public int OverdueCount => Reviews.Count(r => r.Deadline < DateTime.UtcNow);
//        public int DueSoonCount => Reviews.Count(r => r.Deadline <= DateTime.UtcNow.AddDays(3) && r.Deadline >= DateTime.UtcNow);
//    }

//    // Review History ViewModel
//    public class ReviewHistoryViewModel
//    {
//        public List<ReviewDto> Reviews { get; set; } = new();
//        public ReviewerStatisticsDto Statistics { get; set; } = new();
//        public string CurrentUserId { get; set; } = string.Empty;
//    }

//    // Research Summary DTO for Review Forms
//    public class ResearchSummaryDto
//    {
//        public int Id { get; set; }
//        public string Title { get; set; } = string.Empty;
//        public string? TitleEn { get; set; }
//        public string AbstractAr { get; set; } = string.Empty;
//        public string? AbstractEn { get; set; }
//        public ResearchTrack Track { get; set; }
//        public ResearchType ResearchType { get; set; }
//        public DateTime SubmissionDate { get; set; }
//        public List<ResearchAuthorDto> Authors { get; set; } = new();
//        public List<ResearchFileDto> Files { get; set; } = new();

//        // Display Properties
//        public string TrackDisplayName => Track switch
//        {
//            ResearchTrack.InformationTechnology => "تقنية المعلومات",
//            ResearchTrack.InformationSecurity => "أمن المعلومات",
//            ResearchTrack.ArtificialIntelligence => "الذكاء الاصطناعي",
//            ResearchTrack.DataScience => "علوم البيانات",
//            ResearchTrack.SoftwareEngineering => "هندسة البرمجيات",
//            ResearchTrack.NetworkingAndCommunications => "الشبكات والاتصالات",
//            _ => Track.ToString()
//        };

//        public string ResearchTypeDisplayName => ResearchType switch
//        {
//            ResearchType.OriginalResearch => "بحث أصلي",
//            ResearchType.SystematicReview => "مراجعة منهجية",
//            ResearchType.CaseStudy => "دراسة حالة",
//            ResearchType.ExperimentalStudy => "بحث تجريبي",
//            ResearchType.TheoreticalStudy => "بحث نظري",
//            ResearchType.AppliedResearch => "بحث تطبيقي",
//            _ => ResearchType.ToString()
//        };
//    }
//}

//// Statistics DTOs
//namespace ResearchManagement.Application.DTOs
//{
//    public class ReviewStatisticsDto
//    {
//        public int TotalReviews { get; set; }
//        public int CompletedReviews { get; set; }
//        public int PendingReviews { get; set; }
//        public int OverdueReviews { get; set; }
//        public double AverageScore { get; set; }
//        public double CompletionRate => TotalReviews > 0 ? (double)CompletedReviews / TotalReviews * 100 : 0;
//    }

//    public class ReviewerStatisticsDto
//    {
//        public string ReviewerId { get; set; } = string.Empty;
//        public string ReviewerName { get; set; } = string.Empty;
//        public int TotalReviews { get; set; }
//        public int TotalAssigned { get; set; }
//        public int CompletedReviews { get; set; }
//        public int PendingReviews { get; set; }
//        public int OverdueReviews { get; set; }
//        public double AverageReviewTime { get; set; }
//        public double AcceptanceRate { get; set; }
//        public double AverageScore { get; set; }
//        public bool IsActive { get; set; }
//        public double CompletionRate => TotalAssigned > 0 ? (double)CompletedReviews / TotalAssigned * 100 : 0;
//    }

//    public class PagedList<T>
//    {
//        public List<T> Items { get; set; } = new();
//        public int Page { get; set; }
//        public int PageSize { get; set; }
//        public int TotalCount { get; set; }
//        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
//        public bool HasPreviousPage => Page > 1;
//        public bool HasNextPage => Page < TotalPages;
//    }
//}


////using ResearchManagement.Application.DTOs;
////using ResearchManagement.Application.Queries.Research;
////using ResearchManagement.Domain.Enums;
////using Microsoft.AspNetCore.Mvc.Rendering;

////namespace ResearchManagement.Web.Models.ViewModels.Review
////{
////    public class ReviewListViewModel
////    {
////        public PagedResult<ReviewItemViewModel> Reviews { get; set; } = new();
////        public ReviewFilterViewModel Filter { get; set; } = new();
////        public ReviewStatisticsViewModel Statistics { get; set; } = new();
////        public List<SelectListItem> TrackOptions { get; set; } = new();
////        public string CurrentUserId { get; set; } = string.Empty;
////        public UserRole CurrentUserRole { get; set; }
////        public bool CanCreateReview { get; set; }
////        public bool CanManageReviews { get; set; }
////    }

////    public class ReviewFilterViewModel
////    {
////        public string? SearchTerm { get; set; }
////        public string? Status { get; set; }
////        public string? Track { get; set; }
////        public DateTime? FromDate { get; set; }
////        public DateTime? ToDate { get; set; }
////        public int Page { get; set; } = 1;
////        public int PageSize { get; set; } = 10;
////        public string SortBy { get; set; } = "AssignedDate";
////        public bool SortDescending { get; set; } = true;
////    }

////    public class ReviewItemViewModel
////    {
////        public int Id { get; set; }
////        public int ResearchId { get; set; }
////        public string ResearchTitle { get; set; } = string.Empty;
////        public string? ResearchTitleEn { get; set; }
////        public string ResearchAuthor { get; set; } = string.Empty;
////        public ResearchTrack Track { get; set; }
////        public string ReviewerId { get; set; } = string.Empty;
////        public string ReviewerName { get; set; } = string.Empty;
////        public DateTime AssignedDate { get; set; }
////        public DateTime? DueDate { get; set; }
////        public DateTime? CompletedDate { get; set; }
////        public bool IsCompleted { get; set; }
////        public bool IsOverdue => DueDate.HasValue && !IsCompleted && DateTime.UtcNow > DueDate.Value;
////        public int? Score { get; set; }
////        public ReviewDecision? Decision { get; set; }
////        public string? CommentsToAuthor { get; set; }
////        public string? CommentsToTrackManager { get; set; }

////        public string TrackDisplayName => Track switch
////        {
////            ResearchTrack.InformationTechnology => "تقنية المعلومات",
////            ResearchTrack.InformationSecurity => "أمن المعلومات",
////            ResearchTrack.SoftwareEngineering => "هندسة البرمجيات",
////            ResearchTrack.ArtificialIntelligence => "الذكاء الاصطناعي",
////            ResearchTrack.DataScience => "علوم البيانات",
////            ResearchTrack.NetworkingAndCommunications => "الشبكات والاتصالات",
////            _ => "غير محدد"
////        };

////        public string StatusDisplayName
////        {
////            get
////            {
////                if (IsCompleted) return "مكتملة";
////                if (IsOverdue) return "متأخرة";
////                return "معلقة";
////            }
////        }

////        public string StatusBadgeClass
////        {
////            get
////            {
////                if (IsCompleted) return "badge-success";
////                if (IsOverdue) return "badge-danger";
////                return "badge-warning";
////            }
////        }

////        public string DecisionDisplayName => Decision switch
////        {
////            ReviewDecision.AcceptAsIs => "قبول",
////            ReviewDecision.Reject => "رفض",
////            ReviewDecision.AcceptWithMinorRevisions => "تعديلات طفيفة",
////            ReviewDecision.MajorRevisionsRequired => "تعديلات جوهرية",
////            _ => "غير محدد"
////        };

////        public string DecisionBadgeClass => Decision switch
////        {
////            ReviewDecision.AcceptAsIs => "badge-success",
////            ReviewDecision.Reject => "badge-danger",
////            ReviewDecision.AcceptWithMinorRevisions => "badge-info",
////            ReviewDecision.MajorRevisionsRequired => "badge-warning",
////            _ => "badge-secondary"
////        };

////        public int DaysRemaining
////        {
////            get
////            {
////                if (!DueDate.HasValue || IsCompleted) return 0;
////                return Math.Max(0, (DueDate.Value - DateTime.UtcNow).Days);
////            }
////        }

////        public int DaysOverdue
////        {
////            get
////            {
////                if (!IsOverdue) return 0;
////                return (DateTime.UtcNow - DueDate!.Value).Days;
////            }
////        }
////    }

////    public class ReviewStatisticsViewModel
////    {
////        public int TotalReviews { get; set; }
////        public int PendingReviews { get; set; }
////        public int CompletedReviews { get; set; }
////        public int OverdueReviews { get; set; }
////        public double AverageScore { get; set; }
////        public double CompletionRate => TotalReviews > 0 ? (double)CompletedReviews / TotalReviews * 100 : 0;
////        public double OverdueRate => TotalReviews > 0 ? (double)OverdueReviews / TotalReviews * 100 : 0;

////        // Distribution by decision
////        public int AcceptedCount { get; set; }
////        public int RejectedCount { get; set; }
////        public int MinorRevisionsCount { get; set; }
////        public int MajorRevisionsCount { get; set; }

////        // Time statistics
////        public double AverageReviewTime { get; set; } // in days
////        public int FastestReviewTime { get; set; } // in days
////        public int SlowestReviewTime { get; set; } // in days

////        // Monthly statistics
////        public List<MonthlyReviewStats> MonthlyStats { get; set; } = new();
////    }

////    public class MonthlyReviewStats
////    {
////        public int Year { get; set; }
////        public int Month { get; set; }
////        public int CompletedReviews { get; set; }
////        public double AverageScore { get; set; }
////        public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
////    }
////}