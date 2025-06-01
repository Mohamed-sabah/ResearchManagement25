using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Domain.Entities
{
    public class TrackManager : BaseEntity
    {
        public ResearchTrack Track { get; set; }

        [StringLength(200)]
        public string? TrackDescription { get; set; }

        public bool IsActive { get; set; } = true;

        // Foreign Keys
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Research> ManagedResearches { get; set; } = new List<Research>();
        public virtual ICollection<TrackReviewer> TrackReviewers { get; set; } = new List<TrackReviewer>();
    }
}
