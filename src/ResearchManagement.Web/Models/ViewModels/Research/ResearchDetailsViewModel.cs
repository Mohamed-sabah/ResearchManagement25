using ResearchManagement.Application.DTOs;
using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Web.Models.ViewModels.Research
{
    public class ResearchDetailsViewModel
    {
        public ResearchDto Research { get; set; } = new();
        public List<ResearchFileDto> Files { get; set; } = new();
        public List<ReviewDto> Reviews { get; set; } = new();
        public List<ResearchStatusHistoryDto> StatusHistory { get; set; } = new();
        
        // صلاحيات المستخدم الحالي
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanReview { get; set; }
        public bool CanManageStatus { get; set; }
        public bool CanDownloadFiles { get; set; }
        public bool CanUploadFiles { get; set; }
        
        // معلومات إضافية
        public string CurrentUserId { get; set; } = string.Empty;
        public UserRole CurrentUserRole { get; set; }
        public bool IsAuthor { get; set; }
        public bool IsReviewer { get; set; }
        public bool IsTrackManager { get; set; }
        
        // إحصائيات
        public int TotalReviews { get; set; }
        public int CompletedReviews { get; set; }
        public int PendingReviews { get; set; }
        public double AverageScore { get; set; }
        
        // حالة المراجعة
        public bool AllReviewsCompleted => CompletedReviews == TotalReviews && TotalReviews > 0;
        public bool HasPendingReviews => PendingReviews > 0;
        
        // عرض الحالة
        public string StatusDisplayName => Research.Status switch
        {
            ResearchStatus.Submitted => "مُقدم",
            ResearchStatus.UnderReview => "قيد المراجعة",
            ResearchStatus.Accepted => "مقبول",
            ResearchStatus.Rejected => "مرفوض",
            ResearchStatus.RequiresRevision => "يتطلب تعديل",
            ResearchStatus.Published => "منشور",
            _ => "غير محدد"
        };
        
        public string StatusBadgeClass => Research.Status switch
        {
            ResearchStatus.Submitted => "badge-primary",
            ResearchStatus.UnderReview => "badge-warning",
            ResearchStatus.Accepted => "badge-success",
            ResearchStatus.Rejected => "badge-danger",
            ResearchStatus.RequiresRevision => "badge-info",
            ResearchStatus.Published => "badge-success",
            _ => "badge-secondary"
        };
        
        public string TrackDisplayName => Research.Track switch
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
    }

    public class ResearchStatusHistoryDto
    {
        public int Id { get; set; }
        public ResearchStatus FromStatus { get; set; }
        public ResearchStatus ToStatus { get; set; }
        public DateTime ChangedAt { get; set; }
        public string ChangedByName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        
        public string FromStatusDisplayName => FromStatus switch
        {
            ResearchStatus.Submitted => "مُقدم",
            ResearchStatus.UnderReview => "قيد المراجعة",
            ResearchStatus.Accepted => "مقبول",
            ResearchStatus.Rejected => "مرفوض",
            ResearchStatus.RequiresRevision => "يتطلب تعديل",
            ResearchStatus.Published => "منشور",
            _ => "غير محدد"
        };
        
        public string ToStatusDisplayName => ToStatus switch
        {
            ResearchStatus.Submitted => "مُقدم",
            ResearchStatus.UnderReview => "قيد المراجعة",
            ResearchStatus.Accepted => "مقبول",
            ResearchStatus.Rejected => "مرفوض",
            ResearchStatus.RequiresRevision => "يتطلب تعديل",
            ResearchStatus.Published => "منشور",
            _ => "غير محدد"
        };
    }
}