using ResearchManagement.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ResearchManagement.Web.Models.ViewModels.Research
{
    public class CreateResearchViewModel
    {
        [Required(ErrorMessage = "عنوان البحث مطلوب")]
        [StringLength(500, ErrorMessage = "عنوان البحث يجب أن يكون أقل من 500 حرف")]
        [Display(Name = "عنوان البحث")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "العنوان الإنجليزي يجب أن يكون أقل من 500 حرف")]
        [Display(Name = "العنوان الإنجليزي")]
        public string? TitleEn { get; set; }

        [Required(ErrorMessage = "الملخص العربي مطلوب")]
        [StringLength(2000, ErrorMessage = "الملخص العربي يجب أن يكون أقل من 2000 حرف")]
        [Display(Name = "الملخص العربي")]
        public string AbstractAr { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "الملخص الإنجليزي يجب أن يكون أقل من 2000 حرف")]
        [Display(Name = "الملخص الإنجليزي")]
        public string? AbstractEn { get; set; }

        [Required(ErrorMessage = "الكلمات المفتاحية مطلوبة")]
        [StringLength(500, ErrorMessage = "الكلمات المفتاحية يجب أن تكون أقل من 500 حرف")]
        [Display(Name = "الكلمات المفتاحية")]
        public string Keywords { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "الكلمات المفتاحية الإنجليزية يجب أن تكون أقل من 500 حرف")]
        [Display(Name = "الكلمات المفتاحية الإنجليزية")]
        public string? KeywordsEn { get; set; }

        [Required(ErrorMessage = "نوع البحث مطلوب")]
        [Display(Name = "نوع البحث")]
        public ResearchType ResearchType { get; set; }

        [Required(ErrorMessage = "لغة البحث مطلوبة")]
        [Display(Name = "لغة البحث")]
        public ResearchLanguage Language { get; set; }

        [Required(ErrorMessage = "مسار البحث مطلوب")]
        [Display(Name = "مسار البحث")]
        public ResearchTrack Track { get; set; }

        [Display(Name = "المنهجية")]
        [StringLength(2000, ErrorMessage = "المنهجية يجب أن تكون أقل من 2000 حرف")]
        public string? Methodology { get; set; }

        [Display(Name = "ملاحظات إضافية")]
        [StringLength(1000, ErrorMessage = "الملاحظات يجب أن تكون أقل من 1000 حرف")]
        public string? Notes { get; set; }

        // المؤلفون
        [Display(Name = "المؤلفون")]
        public List<ResearchAuthorViewModel> Authors { get; set; } = new();

        // الملفات
        [Display(Name = "ملفات البحث")]
        public List<IFormFile> Files { get; set; } = new();

        // قوائم الخيارات
        public List<SelectListItem> ResearchTypeOptions { get; set; } = new();
        public List<SelectListItem> LanguageOptions { get; set; } = new();
        public List<SelectListItem> TrackOptions { get; set; } = new();

        // معلومات إضافية
        public string CurrentUserId { get; set; } = string.Empty;
        public bool IsEditMode { get; set; }
        public int? ResearchId { get; set; }

        public CreateResearchViewModel()
        {
            InitializeOptions();
            Authors.Add(new ResearchAuthorViewModel { Order = 1, IsCorresponding = true });
        }

        private void InitializeOptions()
        {
            ResearchTypeOptions = Enum.GetValues<ResearchType>()
                .Select(x => new SelectListItem
                {
                    Value = ((int)x).ToString(),
                    Text = GetResearchTypeDisplayName(x)
                }).ToList();

            LanguageOptions = Enum.GetValues<ResearchLanguage>()
                .Select(x => new SelectListItem
                {
                    Value = ((int)x).ToString(),
                    Text = GetLanguageDisplayName(x)
                }).ToList();

            TrackOptions = Enum.GetValues<ResearchTrack>()
                .Select(x => new SelectListItem
                {
                    Value = ((int)x).ToString(),
                    Text = GetTrackDisplayName(x)
                }).ToList();
        }

        private static string GetResearchTypeDisplayName(ResearchType type) => type switch
        {
            ResearchType.OriginalResearch => "بحث كامل",
            ResearchType.CaseStudy => "بحث قصير",
            ResearchType.ExperimentalStudy => "ملصق علمي",
            ResearchType.AppliedResearch => "ورشة عمل",
            _ => type.ToString()
        };

        private static string GetLanguageDisplayName(ResearchLanguage language) => language switch
        {
            ResearchLanguage.Arabic => "العربية",
            ResearchLanguage.English => "الإنجليزية",
            ResearchLanguage.Bilingual => "ثنائي اللغة",
            _ => language.ToString()
        };

        private static string GetTrackDisplayName(ResearchTrack track) => track switch
        {
            ResearchTrack.InformationTechnology => "تقنية المعلومات",
            ResearchTrack.InformationSecurity => "أمن المعلومات",
            ResearchTrack.SoftwareEngineering => "هندسة البرمجيات",
            ResearchTrack.ArtificialIntelligence => "الذكاء الاصطناعي",
            ResearchTrack.DataScience => "علوم البيانات",
            ResearchTrack.NetworkingAndCommunications => "الشبكات والاتصالات",
            _ => track.ToString()
        };
    }

    public class ResearchAuthorViewModel
    {
        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        [StringLength(100, ErrorMessage = "الاسم الأول يجب أن يكون أقل من 100 حرف")]
        [Display(Name = "الاسم الأول")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم العائلة مطلوب")]
        [StringLength(100, ErrorMessage = "اسم العائلة يجب أن يكون أقل من 100 حرف")]
        [Display(Name = "اسم العائلة")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "الاسم الأول الإنجليزي يجب أن يكون أقل من 100 حرف")]
        [Display(Name = "الاسم الأول الإنجليزي")]
        public string? FirstNameEn { get; set; }

        [StringLength(100, ErrorMessage = "اسم العائلة الإنجليزي يجب أن يكون أقل من 100 حرف")]
        [Display(Name = "اسم العائلة الإنجليزي")]
        public string? LastNameEn { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [StringLength(200, ErrorMessage = "البريد الإلكتروني يجب أن يكون أقل من 200 حرف")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "المؤسسة يجب أن تكون أقل من 200 حرف")]
        [Display(Name = "المؤسسة")]
        public string? Institution { get; set; }

        [StringLength(100, ErrorMessage = "الدرجة العلمية يجب أن تكون أقل من 100 حرف")]
        [Display(Name = "الدرجة العلمية")]
        public string? AcademicDegree { get; set; }

        [StringLength(50, ErrorMessage = "معرف ORCID يجب أن يكون أقل من 50 حرف")]
        [Display(Name = "معرف ORCID")]
        public string? OrcidId { get; set; }

        [Required(ErrorMessage = "ترتيب المؤلف مطلوب")]
        [Range(1, 20, ErrorMessage = "ترتيب المؤلف يجب أن يكون بين 1 و 20")]
        [Display(Name = "الترتيب")]
        public int Order { get; set; }

        [Display(Name = "المؤلف المراسل")]
        public bool IsCorresponding { get; set; }

        public string? UserId { get; set; }

        public string FullName => $"{FirstName} {LastName}";
        public string FullNameEn => !string.IsNullOrEmpty(FirstNameEn) && !string.IsNullOrEmpty(LastNameEn) 
            ? $"{FirstNameEn} {LastNameEn}" 
            : FullName;
    }
}