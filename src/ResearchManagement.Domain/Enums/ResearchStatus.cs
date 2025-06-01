using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ResearchManagement.Domain.Enums
{
    public enum ResearchStatus
    {
        [Display(Name = "مقدم")]
        Submitted = 1,

        [Display(Name = "قيد المراجعة الأولية")]
        UnderInitialReview = 2,

        [Display(Name = "موزع للتقييم")]
        AssignedForReview = 3,

        [Display(Name = "قيد التقييم")]
        UnderReview = 4,

        [Display(Name = "تحت المراجعة")]
        UnderEvaluation = 5,

        [Display(Name = "يتطلب تعديلات طفيفة")]
        RequiresMinorRevisions = 6,

        [Display(Name = "يتطلب تعديلات جوهرية")]
        RequiresMajorRevisions = 7,

        [Display(Name = "تعديلات مقدمة")]
        RevisionsSubmitted = 8,

        [Display(Name = "مراجعة التعديلات")]
        RevisionsUnderReview = 9,

        [Display(Name = "مقبول")]
        Accepted = 10,

        [Display(Name = "مرفوض")]
        Rejected = 11,

        [Display(Name = "منسحب")]
        Withdrawn = 12,

        [Display(Name = "بانتظار المقيم الرابع")]
        AwaitingFourthReviewer = 13
    }
}
