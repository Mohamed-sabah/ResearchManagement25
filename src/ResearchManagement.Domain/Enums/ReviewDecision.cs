using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ResearchManagement.Domain.Enums
{
    public enum ReviewDecision
    {
        [Display(Name = "لم يتم المراجعة بعد")]
        NotReviewed = 0,

        [Display(Name = "قبول فوري")]
        AcceptAsIs = 1,

        [Display(Name = "قبول مع تعديلات طفيفة")]
        AcceptWithMinorRevisions = 2,

        [Display(Name = "تعديلات جوهرية مطلوبة")]
        MajorRevisionsRequired = 3,

        [Display(Name = "رفض")]
        Reject = 4,

        [Display(Name = "غير مناسب للمؤتمر")]
        NotSuitableForConference = 5
    }
}
