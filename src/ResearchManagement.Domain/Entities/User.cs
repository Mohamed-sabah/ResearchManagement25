using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using ResearchManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ResearchManagement.Domain.Entities
{
    public class User : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? FirstNameEn { get; set; }

        [StringLength(100)]
        public string? LastNameEn { get; set; }

        [StringLength(200)]
        public string? Institution { get; set; }

        [StringLength(100)]
        public string? AcademicDegree { get; set; }

        [StringLength(50)]
        public string? OrcidId { get; set; }

        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
      
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<Research> Researches { get; set; } = new List<Research>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<TrackManager> ManagedTracks { get; set; } = new List<TrackManager>();
        public virtual ICollection<ResearchAuthor> AuthoredResearches { get; set; } = new List<ResearchAuthor>();
    }
}
