using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Domain.Entities
{
    public class ResearchStatusHistory : BaseEntity
    {
        public ResearchStatus FromStatus { get; set; }
        public ResearchStatus ToStatus { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public int ResearchId { get; set; }
        [Required]
        public string ChangedById { get; set; } = string.Empty;

        // Navigation Properties
        public virtual Research Research { get; set; } = null!;
        public virtual User ChangedBy { get; set; } = null!;
    }
}
