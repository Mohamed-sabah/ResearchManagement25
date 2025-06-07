using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using ResearchManagement.Application.DTOs;


namespace ResearchManagement.Application.Validators
{
    public class CreateResearchDtoValidator : AbstractValidator<CreateResearchDto>
    {
        public CreateResearchDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("عنوان البحث مطلوب")
                .MaximumLength(500).WithMessage("عنوان البحث يجب ألا يتجاوز 500 حرف");

            RuleFor(x => x.TitleEn)
                .MaximumLength(500).WithMessage("العنوان الإنجليزي يجب ألا يتجاوز 500 حرف")
                .When(x => !string.IsNullOrEmpty(x.TitleEn));

            RuleFor(x => x.AbstractAr)
                .NotEmpty().WithMessage("الملخص باللغة العربية مطلوب")
                .MaximumLength(2000).WithMessage("الملخص يجب ألا يتجاوز 2000 حرف")
                .Must(BeValidWordCount).WithMessage("الملخص يجب أن يكون بين 150-300 كلمة");

            RuleFor(x => x.AbstractEn)
                .MaximumLength(2000).WithMessage("الملخص الإنجليزي يجب ألا يتجاوز 2000 حرف")
                .Must(BeValidWordCountEn).WithMessage("الملخص الإنجليزي يجب أن يكون بين 150-300 كلمة")
                .When(x => !string.IsNullOrEmpty(x.AbstractEn));

            RuleFor(x => x.Keywords)
                .MaximumLength(500).WithMessage("الكلمات المفتاحية يجب ألا تتجاوز 500 حرف")
                .Must(BeValidKeywords).WithMessage("يجب أن تحتوي على 3-8 كلمات مفتاحية")
                .When(x => !string.IsNullOrEmpty(x.Keywords));

            RuleFor(x => x.KeywordsEn)
                .MaximumLength(500).WithMessage("الكلمات المفتاحية الإنجليزية يجب ألا تتجاوز 500 حرف")
                .Must(BeValidKeywords).WithMessage("يجب أن تحتوي على 3-8 كلمات مفتاحية")
                .When(x => !string.IsNullOrEmpty(x.KeywordsEn));

            RuleFor(x => x.ResearchType)
                .IsInEnum().WithMessage("نوع البحث غير صحيح");

            RuleFor(x => x.Language)
                .IsInEnum().WithMessage("لغة البحث غير صحيحة");

            RuleFor(x => x.Track)
                .IsInEnum().WithMessage("التخصص غير صحيح");

            RuleFor(x => x.Methodology)
                .MaximumLength(200).WithMessage("المنهجية يجب ألا تتجاوز 200 حرف")
                .When(x => !string.IsNullOrEmpty(x.Methodology));

            RuleFor(x => x.Authors)
                .NotEmpty().WithMessage("يجب إضافة باحث واحد على الأقل")
                .Must(HaveAtLeastOneCorrespondingAuthor).WithMessage("يجب تحديد باحث مراسل واحد على الأقل");

            RuleForEach(x => x.Authors).SetValidator(new CreateResearchAuthorDtoValidator());
        }
        private bool BeValidWordCount(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            // تنظيف النص العربي
            var cleanText = text.Trim()
                .Replace("،", " ")
                .Replace("؛", " ")
                .Replace(":", " ")
                .Replace(".", " ");

            var words = cleanText.Split(new[] { ' ', '\t', '\n', '\r' },
                StringSplitOptions.RemoveEmptyEntries);

            var wordCount = words.Where(w => w.Length > 1).Count();
            return wordCount >= 150 && wordCount <= 300;
        }
        //private bool BeValidWordCount(string text)
        //{
        //    if (string.IsNullOrWhiteSpace(text)) return false;
        //    var wordCount = text.Trim().Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        //    return wordCount >= 150 && wordCount <= 300;
        //}

        private bool BeValidWordCountEn(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return true; // اختياري
            var wordCount = text.Trim().Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            return wordCount >= 150 && wordCount <= 300;
        }

        private bool BeValidKeywords(string keywords)
        {
            if (string.IsNullOrWhiteSpace(keywords)) return true; // اختياري
            var keywordList = keywords.Split(',').Select(k => k.Trim()).Where(k => !string.IsNullOrEmpty(k));
            var count = keywordList.Count();
            return count >= 3 && count <= 8;
        }

        private bool HaveAtLeastOneCorrespondingAuthor(IList<CreateResearchAuthorDto> authors)
        {
            return authors.Any(a => a.IsCorresponding);
        }

    }
}
