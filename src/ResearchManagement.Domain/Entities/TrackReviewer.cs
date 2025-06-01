using ResearchManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchManagement.Domain.Entities
{
    public class TrackReviewer : BaseEntity
    {
        public ResearchTrack Track { get; set; }
        public bool IsActive { get; set; } = true;

        // Foreign Keys
        public int TrackManagerId { get; set; }
        [Required]
        public string ReviewerId { get; set; } = string.Empty;

        // Navigation Properties
        public virtual TrackManager TrackManager { get; set; } = null!;
        public virtual User Reviewer { get; set; } = null!;
    }
}
