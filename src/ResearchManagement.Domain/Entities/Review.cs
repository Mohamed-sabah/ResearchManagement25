using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ResearchManagement.Domain.Enums;
namespace ResearchManagement.Domain.Entities
{
    public class Review : BaseEntity
    {
        public ReviewDecision Decision { get; set; }

        [Range(1, 10)]
        public int OriginalityScore { get; set; }

        [Range(1, 10)]
        public int MethodologyScore { get; set; }

        [Range(1, 10)]
        public int ClarityScore { get; set; }

        [Range(1, 10)]
        public int SignificanceScore { get; set; }

        [Range(1, 10)]
        public int ReferencesScore { get; set; }

        public decimal OverallScore => (OriginalityScore * 0.2m + MethodologyScore * 0.25m +
                                      ClarityScore * 0.2m + SignificanceScore * 0.2m +
                                      ReferencesScore * 0.15m);

        [StringLength(2000)]
        public string? CommentsToAuthor { get; set; }

        [StringLength(2000)]
        public string? CommentsToTrackManager { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedDate { get; set; }
        public DateTime Deadline { get; set; }

        public bool IsCompleted { get; set; } = false;
        public bool RequiresReReview { get; set; } = false;

        // Foreign Keys
        public int ResearchId { get; set; }
        [Required]
        public string ReviewerId { get; set; } = string.Empty;

        // Navigation Properties
        public virtual Research Research { get; set; } = null!;
        public virtual User Reviewer { get; set; } = null!;
        public virtual ICollection<ResearchFile> ReviewFiles { get; set; } = new List<ResearchFile>();
    }
}
