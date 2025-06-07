using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ResearchManagement.Domain.Enums;
namespace ResearchManagement.Domain.Entities
{
    public class EmailNotification : BaseEntity
    {
        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string ToEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        public NotificationType Type { get; set; }
        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

        public DateTime? SentAt { get; set; }
        public int RetryCount { get; set; } = 0;

        [StringLength(500)]
        public string? ErrorMessage { get; set; }

        // Foreign Keys
        public int? ResearchId { get; set; }
        public string? UserId { get; set; }

        // Navigation Properties
        public virtual Research? Research { get; set; }
        public virtual User? User { get; set; }
    }
}
