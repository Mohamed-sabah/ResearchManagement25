using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ResearchManagement.Domain.Enums
{
    public enum FileType
    {
        [Display(Name = "البحث الأصلي")]
        OriginalResearch = 1,

        [Display(Name = "النسخة المعدلة")]
        RevisedVersion = 2,

        [Display(Name = "ملفات التعديل")]
        RevisionFiles = 3,

        [Display(Name = "ملاحظات المقيم")]
        ReviewerNotes = 4,

        [Display(Name = "ملفات إضافية")]
        SupplementaryFiles = 5,

        [Display(Name = "ملف الاستجابة للمراجعين")]
        ResponseToReviewers = 6
    }
}
