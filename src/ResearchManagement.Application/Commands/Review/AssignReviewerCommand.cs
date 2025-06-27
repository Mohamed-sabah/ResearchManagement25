using MediatR;
using ResearchManagement.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchManagement.Application.Commands.Review
{
    public class AssignReviewerCommand : IRequest<int>
    {
        public int ResearchId { get; set; }
        public string ReviewerId { get; set; } = string.Empty;
        public string TrackManagerId { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
    }

    public class AssignReviewerCommandHandler : IRequestHandler<AssignReviewerCommand, int>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IResearchRepository _researchRepository;
        private readonly ITrackManagerRepository _trackManagerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public AssignReviewerCommandHandler(
            IReviewRepository reviewRepository,
            IResearchRepository researchRepository,
            ITrackManagerRepository trackManagerRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService)
        {
            _reviewRepository = reviewRepository;
            _researchRepository = researchRepository;
            _trackManagerRepository = trackManagerRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<int> Handle(AssignReviewerCommand request, CancellationToken cancellationToken)
        {
            // التحقق من البحث
            var research = await _researchRepository.GetByIdAsync(request.ResearchId);
            if (research == null)
                throw new ArgumentException("البحث غير موجود");

            // التحقق من صلاحيات مدير المسار
            var trackManager = await _trackManagerRepository.GetByUserIdAndTrackAsync(
                request.TrackManagerId, research.Track);
            if (trackManager == null)
                throw new UnauthorizedAccessException("غير مصرح لك بتعيين مراجعين لهذا المسار");

            // التحقق من عدم وجود تعيين سابق
            var existingReview = await _reviewRepository.GetByResearchAndReviewerAsync(
                request.ResearchId, request.ReviewerId);
            if (existingReview != null)
                throw new InvalidOperationException("المراجع معين مسبقاً لهذا البحث");

            // إنشاء تكليف المراجعة
            var review = new Domain.Entities.Review
            {
                ResearchId = request.ResearchId,
                ReviewerId = request.ReviewerId,
                AssignedDate = DateTime.UtcNow,
                Deadline = request.Deadline ?? DateTime.UtcNow.AddDays(14),
                Decision = Domain.Enums.ReviewDecision.NotReviewed,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.TrackManagerId
            };

            await _reviewRepository.AddAsync(review);

            // تحديث حالة البحث
            if (research.Status == Domain.Enums.ResearchStatus.Submitted)
            {
                research.Status = Domain.Enums.ResearchStatus.AssignedForReview;
                research.UpdatedAt = DateTime.UtcNow;
                research.UpdatedBy = request.TrackManagerId;
                await _researchRepository.UpdateAsync(research);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // إرسال إشعار للمراجع
            try
            {
                await _emailService.SendReviewAssignmentNotificationAsync(review.Id);
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ لكن لا توقف العملية
            }

            return review.Id;
        }
    }
}