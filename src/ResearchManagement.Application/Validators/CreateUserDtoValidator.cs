using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using ResearchManagement.Application.DTOs;

namespace ResearchManagement.Application.Validators
{
    public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserDtoValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("اسم المستخدم مطلوب")
                .Length(3, 50).WithMessage("اسم المستخدم يجب أن يكون بين 3 و 50 حرف");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
                .EmailAddress().WithMessage("البريد الإلكتروني غير صحيح")
                .MaximumLength(200).WithMessage("البريد الإلكتروني يجب ألا يتجاوز 200 حرف");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("كلمة المرور مطلوبة")
                .MinimumLength(8).WithMessage("كلمة المرور يجب أن تكون 8 أحرف على الأقل")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage("كلمة المرور يجب أن تحتوي على أحرف كبيرة وصغيرة وأرقام");

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

            RuleFor(x => x.Institution)
                .MaximumLength(200).WithMessage("اسم المؤسسة يجب ألا يتجاوز 200 حرف")
                .When(x => !string.IsNullOrEmpty(x.Institution));

            RuleFor(x => x.AcademicDegree)
                .MaximumLength(100).WithMessage("الدرجة العلمية يجب ألا تتجاوز 100 حرف")
                .When(x => !string.IsNullOrEmpty(x.AcademicDegree));

            RuleFor(x => x.OrcidId)
                .MaximumLength(50).WithMessage("رقم ORCID يجب ألا يتجاوز 50 حرف")
                .Matches(@"^\d{4}-\d{4}-\d{4}-\d{4}$").WithMessage("تنسيق رقم ORCID غير صحيح")
                .When(x => !string.IsNullOrEmpty(x.OrcidId));

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("نوع المستخدم غير صحيح");
        }
    }
}
