using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Web.Models.ViewModels
{
    public class ResearchListViewModel
    {
        public List<Research> Researches { get; set; } = new();
        public ResearchFilterModel Filter { get; set; } = new();
        public PaginationModel Pagination { get; set; } = new();
        public bool CanCreateNew { get; set; } = true;
        public string ViewTitle { get; set; } = "البحوث";
    }

    public class ResearchFilterModel
    {
        public ResearchStatus? Status { get; set; }
        public ResearchTrack? Track { get; set; }
        public string? SearchTerm { get; set; }
        public DateTime? SubmissionDateFrom { get; set; }
        public DateTime? SubmissionDateTo { get; set; }
        public string? AuthorName { get; set; }
    }

    public class PaginationModel
    {
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
