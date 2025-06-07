using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ResearchManagement.Domain.Enums
{
    public enum UserRole
    {
        [Display(Name = "باحث")]
        Researcher = 1,

        [Display(Name = "مقيم")]
        Reviewer = 2,

        [Display(Name = "مدير التراك")]
        TrackManager = 3,

        [Display(Name = "مدير المؤتمر")]
        ConferenceManager = 4,

        [Display(Name = "مدير النظام")]
        SystemAdmin = 5
    }
}
