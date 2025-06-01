using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ResearchManagement.Domain.Enums
{
    public enum NotificationStatus
    {
        [Display(Name = "بانتظار الإرسال")]
        Pending = 1,

        [Display(Name = "تم الإرسال")]
        Sent = 2,

        [Display(Name = "فشل الإرسال")]
        Failed = 3,

        [Display(Name = "تم الإلغاء")]
        Cancelled = 4
    }
}
