using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using ResearchManagement.Application.Interfaces;

namespace ResearchManagement.Application.Commands.Review
{
    public class CompleteReviewCommand : IRequest<bool>
    {
        public int ReviewId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public class CompleteReviewCommandHandler : IRequestHandler<CompleteReviewCommand, bool>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public CompleteReviewCommandHandler(
            IReviewRepository reviewRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService)
        {
            _reviewRepository = reviewRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<bool> Handle(CompleteReviewCommand request, CancellationToken cancellationToken)
        {
            var review = await _reviewRepository.GetByIdAsync(request.ReviewId);
            if (review == null)
                return false;

            // التحقق من الصلاحيات
            if (review.ReviewerId != request.UserId)
                throw new UnauthorizedAccessException("غير مصرح لك بإكمال هذه المراجعة");

            if (review.IsCompleted)
                return true; // مكتملة مسبقاً

            // تحديث حالة المراجعة
            review.IsCompleted = true;
            review.CompletedDate = DateTime.UtcNow;
            review.UpdatedAt = DateTime.UtcNow;
            review.UpdatedBy = request.UserId;

            await _reviewRepository.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // إرسال إشعار بإكمال المراجعة
            try
            {
                await _emailService.SendReviewCompletedNotificationAsync(review.Id);
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ لكن لا توقف العملية
            }

            return true;
        }
    }
}