using MediatR;
using ResearchManagement.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchManagement.Application.Commands.TrackManager
{
    public class RemoveReviewerCommand : IRequest<bool>
    {
        public int ReviewId { get; set; }
        public string TrackManagerId { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }

    public class RemoveReviewerCommandHandler : IRequestHandler<RemoveReviewerCommand, bool>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly ITrackManagerRepository _trackManagerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public RemoveReviewerCommandHandler(
            IReviewRepository reviewRepository,
            ITrackManagerRepository trackManagerRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService)
        {
            _reviewRepository = reviewRepository;
            _trackManagerRepository = trackManagerRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<bool> Handle(RemoveReviewerCommand request, CancellationToken cancellationToken)
        {
            var review = await _reviewRepository.GetByIdWithDetailsAsync(request.ReviewId);
            if (review == null)
                return false;

            // التحقق من الصلاحيات
            var trackManager = await _trackManagerRepository.GetByUserIdAndTrackAsync(
                request.TrackManagerId, review.Research.Track);
            if (trackManager == null)
                throw new UnauthorizedAccessException("غير مصرح لك بإزالة هذا المراجع");

            // لا يمكن إزالة مراجع أكمل المراجعة
            if (review.IsCompleted)
                throw new InvalidOperationException("لا يمكن إزالة مراجع أكمل المراجعة");

            // حذف المراجعة (Soft Delete)
            review.IsDeleted = true;
            review.UpdatedAt = DateTime.UtcNow;
            review.UpdatedBy = request.TrackManagerId;

            await _reviewRepository.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // إرسال إشعار للمراجع
            _ = Task.Run(async () => {
                try
                {
                    await _emailService.SendReviewerRemovalNotificationAsync(
                        review.ReviewerId, review.Research.Title, request.Reason);
                }
                catch
                {
                    // تسجيل الخطأ
                }
            });

            return true;
        }
    }
}