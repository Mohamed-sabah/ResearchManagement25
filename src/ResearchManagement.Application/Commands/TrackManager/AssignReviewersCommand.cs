using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Application.Commands.TrackManager
{
    public class AssignReviewersCommand : IRequest<bool>
    {
        public int ResearchId { get; set; }
        public List<string> ReviewerIds { get; set; } = new();
        public string TrackManagerId { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
    }

    public class AssignReviewersCommandHandler : IRequestHandler<AssignReviewersCommand, bool>
    {
        private readonly IResearchRepository _researchRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly ITrackManagerRepository _trackManagerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public AssignReviewersCommandHandler(
            IResearchRepository researchRepository,
            IReviewRepository reviewRepository,
            ITrackManagerRepository trackManagerRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService)
        {
            _researchRepository = researchRepository;
            _reviewRepository = reviewRepository;
            _trackManagerRepository = trackManagerRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<bool> Handle(AssignReviewersCommand request, CancellationToken cancellationToken)
        {
            // التحقق من البحث
            var research = await _researchRepository.GetByIdAsync(request.ResearchId);
            if (research == null)
                throw new ArgumentException("البحث غير موجود");

            // التحقق من صلاحيات مدير المسار
            var trackManager = await _trackManagerRepository.GetByUserIdAndTrackAsync(
                request.TrackManagerId, research.Track);
            if (trackManager == null)
                throw new UnauthorizedAccessException("غير مصرح لك بإدارة هذا المسار");

            var successfulAssignments = new List<int>();

            foreach (var reviewerId in request.ReviewerIds)
            {
                try
                {
                    // التحقق من عدم وجود تعيين سابق
                    var existingReview = await _reviewRepository.GetByResearchAndReviewerAsync(
                        request.ResearchId, reviewerId);

                    if (existingReview != null)
                        continue; // تجاهل المراجعين المعينين مسبقاً

                    // إنشاء تكليف جديد
                    var review = new Domain.Entities.Review
                    {
                        ResearchId = request.ResearchId,
                        ReviewerId = reviewerId,
                        AssignedDate = DateTime.UtcNow,
                        Deadline = request.Deadline ?? DateTime.UtcNow.AddDays(14),
                        Decision = ReviewDecision.NotReviewed,
                        IsCompleted = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = request.TrackManagerId
                    };

                    await _reviewRepository.AddAsync(review);
                    successfulAssignments.Add(review.Id);

                    // إرسال إشعار للمراجع (بدون انتظار)
                    _ = Task.Run(async () => {
                        try
                        {
                            await _emailService.SendReviewAssignmentNotificationAsync(review.Id);
                        }
                        catch
                        {
                            // تسجيل الخطأ
                        }
                    });
                }
                catch
                {
                    // تجاهل الأخطاء الفردية واستمر مع المراجعين الآخرين
                    continue;
                }
            }

            // تحديث حالة البحث إذا تم تعيين مراجعين
            if (successfulAssignments.Any())
            {
                if (research.Status == ResearchStatus.Submitted)
                {
                    research.Status = ResearchStatus.AssignedForReview;
                    research.UpdatedAt = DateTime.UtcNow;
                    research.UpdatedBy = request.TrackManagerId;
                    await _researchRepository.UpdateAsync(research);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return successfulAssignments.Any();
        }
    }
}