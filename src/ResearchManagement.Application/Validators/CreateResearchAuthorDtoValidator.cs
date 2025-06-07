using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using ResearchManagement.Application.DTOs;

namespace ResearchManagement.Application.Validators
{
    public class CreateResearchAuthorDtoValidator : AbstractValidator<CreateResearchAuthorDto>
    {
        public CreateResearchAuthorDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("الاسم الأول مطلوب")
                .MaximumLength(100).WithMessage("الاسم الأول يجب ألا يتجاوز 100 حرف");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("اسم العائلة مطلوب")
                .MaximumLength(100).WithMessage("اسم العائلة يجب ألا يتجاوز 100 حرف");

            RuleFor(x => x.FirstNameEn)
                .MaximumLength(100).WithMessage("الاسم الأول الإنجليزي يجب ألا يتجاوز 100 حرف")
                .When(x => !string.IsNullOrEmpty(x.FirstNameEn));

            RuleFor(x => x.LastNameEn)
                .MaximumLength(100).WithMessage("اسم العائلة الإنجليزي يجب ألا يتجاوز 100 حرف")
                .When(x => !string.IsNullOrEmpty(x.LastNameEn));

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
                .EmailAddress().WithMessage("البريد الإلكتروني غير صحيح")
                .MaximumLength(200).WithMessage("البريد الإلكتروني يجب ألا يتجاوز 200 حرف");

            RuleFor(x => x.Institution)
                .MaximumLength(200).WithMessage("اسم المؤسسة يجب ألا يتجاوز 200 حرف")
                .When(x => !string.IsNullOrEmpty(x.Institution));

            RuleFor(x => x.AcademicDegree)
                .MaximumLength(100).WithMessage("الدرجة العلمية يجب ألا تتجاوز 100 حرف")
                .When(x => !string.IsNullOrEmpty(x.AcademicDegree));

            RuleFor(x => x.OrcidId)
                .MaximumLength(50).WithMessage("رقم ORCID يجب ألا يتجاوز 50 حرف")
                .Matches(@"^\d{4}-\d{4}-\d{4}-\d{4}$").WithMessage("تنسيق رقم ORCID غير صحيح (يجب أن يكون بالشكل: 0000-0000-0000-0000)")
                .When(x => !string.IsNullOrEmpty(x.OrcidId));

            RuleFor(x => x.Order)
                .GreaterThan(0).WithMessage("ترتيب الباحث يجب أن يكون أكبر من صفر");
        }
    }
}
