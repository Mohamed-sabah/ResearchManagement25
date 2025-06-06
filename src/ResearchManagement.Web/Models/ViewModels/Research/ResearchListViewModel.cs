using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Queries.Research;
using ResearchManagement.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ResearchManagement.Web.Models.ViewModels.Research
{
    public class ResearchListViewModel
    {
        public PagedResult<ResearchDto> Researches { get; set; } = new();
        public ResearchFilterViewModel Filter { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
        public List<SelectListItem> TrackOptions { get; set; } = new();
        public string CurrentUserId { get; set; } = string.Empty;
        public UserRole CurrentUserRole { get; set; }
        public bool CanCreateResearch { get; set; }
        public bool CanManageResearches { get; set; }
    }

    public class ResearchFilterViewModel
    {
        public string? SearchTerm { get; set; }
        public ResearchStatus? Status { get; set; }
        public ResearchTrack? Track { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "SubmissionDate";
        public bool SortDescending { get; set; } = true;
    }

    public class ResearchCardViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string AbstractAr { get; set; } = string.Empty;
        public ResearchStatus Status { get; set; }
        public ResearchTrack Track { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string SubmittedByName { get; set; } = string.Empty;
        public List<string> AuthorNames { get; set; } = new();
        public int ReviewsCount { get; set; }
        public int FilesCount { get; set; }
        public bool CanEdit { get; set; }
        public bool CanView { get; set; }
        public bool CanReview { get; set; }
        public bool CanManage { get; set; }
        public string StatusBadgeClass => Status switch
        {
            ResearchStatus.Submitted => "badge-primary",
            ResearchStatus.UnderReview => "badge-warning",
            ResearchStatus.Accepted => "badge-success",
            ResearchStatus.Rejected => "badge-danger",
            ResearchStatus.RequiresMinorRevisions => "badge-info",
            ResearchStatus.RequiresMajorRevisions => "badge-warning",
            _ => "badge-secondary"
        };
        public string StatusDisplayName => Status switch
        {
            ResearchStatus.Submitted => "مُقدم",
            ResearchStatus.UnderReview => "قيد المراجعة",
            ResearchStatus.Accepted => "مقبول",
            ResearchStatus.Rejected => "مرفوض",
            ResearchStatus.RequiresMinorRevisions => "يتطلب تعديلات طفيفة",
            ResearchStatus.RequiresMajorRevisions => "يتطلب تعديلات جوهرية",
            _ => "غير محدد"
        };
    }
}