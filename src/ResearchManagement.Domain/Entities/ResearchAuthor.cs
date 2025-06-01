using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace ResearchManagement.Domain.Entities
{
    public class ResearchAuthor : BaseEntity
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

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Institution { get; set; }

        [StringLength(100)]
        public string? AcademicDegree { get; set; }

        [StringLength(50)]
        public string? OrcidId { get; set; }

        public int Order { get; set; } = 1;
        public bool IsCorresponding { get; set; } = false;

        // Foreign Keys
        public int ResearchId { get; set; }
        public string? UserId { get; set; }

        // Navigation Properties
        public virtual Research Research { get; set; } = null!;
        public virtual User? User { get; set; }
    }
}
