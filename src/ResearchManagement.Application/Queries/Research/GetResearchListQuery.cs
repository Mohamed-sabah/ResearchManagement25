using MediatR;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Application.Queries.Research
{
    public class GetResearchListQuery : IRequest<PagedResult<ResearchDto>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public ResearchStatus? Status { get; set; }
        public ResearchTrack? Track { get; set; }
        public string? UserId { get; set; }
        public UserRole? UserRole { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SortBy { get; set; } = "SubmissionDate";
        public bool SortDescending { get; set; } = true;

        public GetResearchListQuery() { }

        public GetResearchListQuery(string? userId, UserRole? userRole)
        {
            UserId = userId;
            UserRole = userRole;
        }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }
}