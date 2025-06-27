using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchManagement.Application.DTOs
{
    public class TrackDashboardDto
    {
        public string TrackName { get; set; } = string.Empty;
        public ResearchTrack Track { get; set; }
        public TrackStatisticsDto Statistics { get; set; } = new();
        public List<Research> RecentResearches { get; set; } = new();
        public List<Research> PendingResearches { get; set; } = new();
        public List<Research> ResearchesNeedingReviewers { get; set; } = new();
        public List<Review> OverdueReviews { get; set; } = new();
    }

    public class TrackStatisticsDto
    {
        public int TotalResearches { get; set; }
        public int SubmittedResearches { get; set; }
        public int UnderReviewResearches { get; set; }
        public int AcceptedResearches { get; set; }
        public int RejectedResearches { get; set; }
        public int RequiringRevisionResearches { get; set; }
        public double AverageProcessingTime { get; set; } // في الأيام

        // Computed properties
        public double AcceptanceRate => TotalResearches > 0 ?
            (double)AcceptedResearches / TotalResearches * 100 : 0;

        public double RejectionRate => TotalResearches > 0 ?
            (double)RejectedResearches / TotalResearches * 100 : 0;

        public int ProcessingResearches => SubmittedResearches + UnderReviewResearches + RequiringRevisionResearches;
    }

    public class TrackReportDto
    {
        public string TrackName { get; set; } = string.Empty;
        public ResearchTrack Track { get; set; }
        public DateTime ReportGeneratedAt { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalResearches { get; set; }
        public Dictionary<ResearchStatus, int> ResearchesByStatus { get; set; } = new();
        public Dictionary<string, int> ResearchesByMonth { get; set; } = new();
        public double AverageReviewTime { get; set; }
        public List<ReviewerPerformanceDto> ReviewerPerformance { get; set; } = new();
        public Dictionary<ReviewDecision, int> DecisionStatistics { get; set; } = new();
        public Dictionary<string, int> ScoreDistribution { get; set; } = new();
    }

    public class ReviewerPerformanceDto
    {
        public string ReviewerId { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;
        public int TotalReviews { get; set; }
        public int CompletedReviews { get; set; }
        public double AverageScore { get; set; }
        public double AcceptanceRate { get; set; }
        public double AverageReviewTime { get; set; }
    }

    public class ResearchFilterDto
    {
        public string? SearchTerm { get; set; }
        public ResearchStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class ReviewFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public string? Track { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PaginationDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    // DTO for Research Summary in Track context
    public class ResearchSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? TitleEn { get; set; }
        public string AbstractAr { get; set; } = string.Empty;
        public string? AbstractEn { get; set; }
        public ResearchTrack Track { get; set; }
        public ResearchType ResearchType { get; set; }
        public DateTime SubmissionDate { get; set; }
        public List<ResearchAuthorDto> Authors { get; set; } = new();
        public List<ResearchFileDto> Files { get; set; } = new();

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

        public string ResearchTypeDisplayName => ResearchType switch
        {
            ResearchType.OriginalResearch => "بحث أصلي",
            ResearchType.SystematicReview => "مراجعة منهجية",
            ResearchType.CaseStudy => "دراسة حالة",
            ResearchType.ExperimentalStudy => "بحث تجريبي",
            ResearchType.TheoreticalStudy => "بحث نظري",
            ResearchType.AppliedResearch => "بحث تطبيقي",
            ResearchType.LiteratureReview => "مراجعة أدبية",
            ResearchType.ComparativeStudy => "بحث مقارن",
            _ => ResearchType.ToString()
        };
    }
}