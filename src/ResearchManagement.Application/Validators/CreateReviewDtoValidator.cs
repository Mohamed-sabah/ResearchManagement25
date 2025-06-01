using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using ResearchManagement.Application.DTOs;

namespace ResearchManagement.Application.Validators
{
    public class CreateReviewDtoValidator : AbstractValidator<CreateReviewDto>
    {
        public CreateReviewDtoValidator()
        {
            RuleFor(x => x.Decision)
                .IsInEnum().WithMessage("قرار المراجعة غير صحيح");

            RuleFor(x => x.OriginalityScore)
                .InclusiveBetween(1, 10).WithMessage("درجة الأصالة يجب أن تكون بين 1 و 10");

            RuleFor(x => x.MethodologyScore)
                .InclusiveBetween(1, 10).WithMessage("درجة المنهجية يجب أن تكون بين 1 و 10");

            RuleFor(x => x.ClarityScore)
                .InclusiveBetween(1, 10).WithMessage("درجة الوضوح يجب أن تكون بين 1 و 10");

            RuleFor(x => x.SignificanceScore)
                .InclusiveBetween(1, 10).WithMessage("درجة الأهمية يجب أن تكون بين 1 و 10");

            RuleFor(x => x.ReferencesScore)
                .InclusiveBetween(1, 10).WithMessage("درجة المراجع يجب أن تكون بين 1 و 10");

            RuleFor(x => x.CommentsToAuthor)
                .MaximumLength(2000).WithMessage("التعليقات للباحث يجب ألا تتجاوز 2000 حرف")
                .When(x => !string.IsNullOrEmpty(x.CommentsToAuthor));

            RuleFor(x => x.CommentsToTrackManager)
                .MaximumLength(2000).WithMessage("التعليقات لمدير التراك يجب ألا تتجاوز 2000 حرف")
                .When(x => !string.IsNullOrEmpty(x.CommentsToTrackManager));

            RuleFor(x => x.ResearchId)
                .GreaterThan(0).WithMessage("معرف البحث غير صحيح");

            // تحقق من أن المراجعة تحتوي على تعليقات عند الرفض أو طلب التعديل
            RuleFor(x => x.CommentsToAuthor)
                .NotEmpty().WithMessage("يجب إضافة تعليقات للباحث عند رفض البحث أو طلب تعديلات")
                .When(x => x.Decision == Domain.Enums.ReviewDecision.Reject ||
                          x.Decision == Domain.Enums.ReviewDecision.MajorRevisionsRequired);
        }
    }
}
