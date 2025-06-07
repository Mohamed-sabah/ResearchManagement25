using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ResearchManagement.Domain.Enums;
using Microsoft.VisualBasic.FileIO;

namespace ResearchManagement.Domain.Entities
{
    public class ResearchFile : BaseEntity
    {
        [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }
    public FileType FileType { get; set; }
    public int Version { get; set; } = 1;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // Foreign Keys
    public int? ResearchId { get; set; } // مطلوب
    public int? ReviewId { get; set; } // اختياري - هنا الحل!

    // Navigation Properties
    public virtual Research Research { get; set; } = null!;
    public virtual Review? Review { get; set; } // اختياري
}
}
