using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Application.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public ReviewDecision Decision { get; set; }
        public int OriginalityScore { get; set; }
        public int MethodologyScore { get; set; }
        public int ClarityScore { get; set; }
        public int SignificanceScore { get; set; }
        public int ReferencesScore { get; set; }
        public decimal OverallScore { get; set; }
        public string? CommentsToAuthor { get; set; }
        public string? CommentsToTrackManager { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime Deadline { get; set; }
        public bool IsCompleted { get; set; }
        public bool RequiresReReview { get; set; }
        public string ReviewerId { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;
        public int ResearchId { get; set; }
        public string ResearchTitle { get; set; } = string.Empty;
        public List<ResearchFileDto> ReviewFiles { get; set; } = new();
    }

    public class CreateReviewDto
    {
        public ReviewDecision Decision { get; set; }
        public int OriginalityScore { get; set; }
        public int MethodologyScore { get; set; }
        public int ClarityScore { get; set; }
        public int SignificanceScore { get; set; }
        public int ReferencesScore { get; set; }
        public string? CommentsToAuthor { get; set; }
        public string? CommentsToTrackManager { get; set; }
        public bool RequiresReReview { get; set; } = false;
        public int ResearchId { get; set; }
    }

    public class UpdateReviewDto : CreateReviewDto
    {
        public int Id { get; set; }
    }
}
