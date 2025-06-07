using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ResearchManagement.Domain.Enums
{
    public enum ResearchType
    {
        [Display(Name = "بحث أصلي")]
        OriginalResearch = 1,

        [Display(Name = "مراجعة منهجية")]
        SystematicReview = 2,

        [Display(Name = "دراسة حالة")]
        CaseStudy = 3,

        [Display(Name = "بحث تجريبي")]
        ExperimentalStudy = 4,

        [Display(Name = "بحث نظري")]
        TheoreticalStudy = 5,

        [Display(Name = "بحث تطبيقي")]
        AppliedResearch = 6,

        [Display(Name = "مراجعة أدبية")]
        LiteratureReview = 7,

        [Display(Name = "بحث مقارن")]
        ComparativeStudy = 8
    }
}
