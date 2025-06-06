using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Domain.Entities
{
    public class Research : BaseEntity
    {
        public int Id { get; set; }
        [Required]
        [StringLength(500)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? TitleEn { get; set; }

        [Required]
        [StringLength(2000)]
        public string AbstractAr { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? AbstractEn { get; set; }

        [StringLength(500)]
        public string? Keywords { get; set; }

        [StringLength(500)]
        public string? KeywordsEn { get; set; }

        public ResearchType ResearchType { get; set; }
        public ResearchLanguage Language { get; set; }
        public ResearchStatus Status { get; set; } = ResearchStatus.Submitted;
        public ResearchTrack Track { get; set; }

        [StringLength(200)]
        public string? Methodology { get; set; }

        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewDeadline { get; set; }
        public DateTime? DecisionDate { get; set; }

        [StringLength(1000)]
        public string? RejectionReason { get; set; }

        // Foreign Keys
        [Required]
        public string SubmittedById { get; set; } = string.Empty;
        public int? AssignedTrackManagerId { get; set; }

        // Navigation Properties
        public virtual User SubmittedBy { get; set; } = null!;
        public virtual TrackManager? AssignedTrackManager { get; set; }
        public virtual ICollection<ResearchAuthor> Authors { get; set; } = new List<ResearchAuthor>();
        public virtual ICollection<ResearchFile> Files { get; set; } = new List<ResearchFile>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<ResearchStatusHistory> StatusHistory { get; set; } = new List<ResearchStatusHistory>();
    }
}
