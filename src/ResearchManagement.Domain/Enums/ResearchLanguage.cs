using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ResearchManagement.Domain.Enums
{
    public enum ResearchLanguage
    {
        [Display(Name = "العربية")]
        Arabic = 1,

        [Display(Name = "الإنجليزية")]
        English = 2,

        [Display(Name = "ثنائية اللغة")]
        Bilingual = 3
    }
}
