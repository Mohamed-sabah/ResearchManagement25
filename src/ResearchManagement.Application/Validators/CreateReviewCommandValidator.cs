using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation;
using ResearchManagement.Application.Commands.Review;

namespace ResearchManagement.Application.Validators
{
    public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
    {
        public CreateReviewCommandValidator()
        {
            RuleFor(x => x.Review.ResearchId)
                .GreaterThan(0)
                .WithMessage("معرف البحث مطلوب");

            RuleFor(x => x.ReviewerId)
                .NotEmpty()
                .WithMessage("معرف المراجع مطلوب");

            RuleFor(x => x.Review.Decision)
                .IsInEnum()
                .WithMessage("يجب اختيار قرار صحيح");

            RuleFor(x => x.Review.OriginalityScore)
                .InclusiveBetween(1, 10)
                .WithMessage("نقاط الأصالة يجب أن تكون بين 1 و 10");

            RuleFor(x => x.Review.MethodologyScore)
                .InclusiveBetween(1, 10)
                .WithMessage("نقاط المنهجية يجب أن تكون بين 1 و 10");

            RuleFor(x => x.Review.ClarityScore)
                .InclusiveBetween(1, 10)
                .WithMessage("نقاط الوضوح يجب أن تكون بين 1 و 10");

            RuleFor(x => x.Review.SignificanceScore)
                .InclusiveBetween(1, 10)
                .WithMessage("نقاط الأهمية يجب أن تكون بين 1 و 10");

            RuleFor(x => x.Review.ReferencesScore)
                .InclusiveBetween(1, 10)
                .WithMessage("نقاط المراجع يجب أن تكون بين 1 و 10");

            RuleFor(x => x.Review.CommentsToAuthor)
                .NotEmpty()
                .WithMessage("التعليقات للمؤلف مطلوبة")
                .MaximumLength(2000)
                .WithMessage("التعليقات يجب ألا تزيد عن 2000 حرف");

            RuleFor(x => x.Review.CommentsToTrackManager)
                .MaximumLength(2000)
                .WithMessage("التعليقات لمدير المسار يجب ألا تزيد عن 2000 حرف");

            RuleFor(x => x.Review.Recommendations)
                .MaximumLength(1000)
                .WithMessage("التوصيات يجب ألا تزيد عن 1000 حرف");
        }
    }
}
