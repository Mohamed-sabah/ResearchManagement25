using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ResearchManagement.Domain.Enums
{
    public enum ResearchTrack
    {
        [Display(Name = "تقنية المعلومات")]
        InformationTechnology = 1,

        [Display(Name = "أمن المعلومات")]
        InformationSecurity = 2,

        [Display(Name = "الذكاء الاصطناعي")]
        ArtificialIntelligence = 3,

        [Display(Name = "علوم البيانات")]
        DataScience = 4,

        [Display(Name = "هندسة البرمجيات")]
        SoftwareEngineering = 5,

        [Display(Name = "الشبكات والاتصالات")]
        NetworkingAndCommunications = 6,

        [Display(Name = "الحوسبة السحابية")]
        CloudComputing = 7,

        [Display(Name = "إنترنت الأشياء")]
        InternetOfThings = 8,

        [Display(Name = "الواقع المعزز والافتراضي")]
        ARAndVR = 9,

        [Display(Name = "البلوك تشين")]
        Blockchain = 10,

        [Display(Name = "التعلم الآلي")]
        MachineLearning = 11,

        [Display(Name = "معالجة اللغات الطبيعية")]
        NaturalLanguageProcessing = 12,

        [Display(Name = "الحوسبة عالية الأداء")]
        HighPerformanceComputing = 13,

        [Display(Name = "تطوير التطبيقات المحمولة")]
        MobileAppDevelopment = 14,

        [Display(Name = "قواعد البيانات")]
        DatabaseSystems = 15
    }
}
