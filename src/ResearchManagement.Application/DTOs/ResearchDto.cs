using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Application.DTOs
{
    public class ResearchDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? TitleEn { get; set; }
        public string AbstractAr { get; set; } = string.Empty;
        public string? AbstractEn { get; set; }
        public string? Keywords { get; set; }
        public string? KeywordsEn { get; set; }
        public ResearchType ResearchType { get; set; }
        public ResearchLanguage Language { get; set; }
        public ResearchStatus Status { get; set; }
        public ResearchTrack Track { get; set; }
        public string? Methodology { get; set; }
        public DateTime SubmissionDate { get; set; }
        public DateTime? ReviewDeadline { get; set; }
        public DateTime? DecisionDate { get; set; }
        public string? RejectionReason { get; set; }
        public string SubmittedById { get; set; } = string.Empty;
        public string SubmittedByName { get; set; } = string.Empty;
        public List<ResearchAuthorDto> Authors { get; set; } = new();
        public List<ResearchFileDto> Files { get; set; } = new();
        public List<ReviewDto> Reviews { get; set; } = new();
    }

    public class CreateResearchDto
    {
        public string Title { get; set; } = string.Empty;
        public string? TitleEn { get; set; }
        public string AbstractAr { get; set; } = string.Empty;
        public string? AbstractEn { get; set; }
        public string? Keywords { get; set; }
        public string? KeywordsEn { get; set; }
        public ResearchType ResearchType { get; set; }
        public ResearchLanguage Language { get; set; }
        public ResearchTrack Track { get; set; }
        public string? Methodology { get; set; }
        public List<CreateResearchAuthorDto> Authors { get; set; } = new();
    }

    public class UpdateResearchDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? TitleEn { get; set; }
        public string AbstractAr { get; set; } = string.Empty;
        public string? AbstractEn { get; set; }
        public string? Keywords { get; set; }
        public string? KeywordsEn { get; set; }
        public ResearchType ResearchType { get; set; }
        public ResearchLanguage Language { get; set; }
        public ResearchTrack Track { get; set; }
        public string? Methodology { get; set; }
        public List<UpdateResearchAuthorDto> Authors { get; set; } = new();
    }
}
