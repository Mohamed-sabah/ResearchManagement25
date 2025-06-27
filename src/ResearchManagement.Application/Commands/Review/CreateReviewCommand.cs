using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

using MediatR;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Entities;

namespace ResearchManagement.Application.Commands.Review
{
    public class CreateReviewCommand : IRequest<int>
    {
        public CreateReviewDto Review { get; set; } = new();
        public string ReviewerId { get; set; } = string.Empty;
    }

    public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, int>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IResearchRepository _researchRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public CreateReviewCommandHandler(
            IReviewRepository reviewRepository,
            IResearchRepository researchRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IMapper mapper)
        {
            _reviewRepository = reviewRepository;
            _researchRepository = researchRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<int> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            // التحقق من وجود مراجعة سابقة للمراجع نفسه
            var existingReview = await _reviewRepository.GetByResearchAndReviewerAsync(
                request.Review.ResearchId, request.ReviewerId);

            if (existingReview != null)
                throw new InvalidOperationException("لقد تم تعيينك مسبقاً لمراجعة هذا البحث");

            // إنشاء المراجعة
            var review = _mapper.Map<Domain.Entities.Review>(request.Review);
            review.ReviewerId = request.ReviewerId;
            review.AssignedDate = DateTime.UtcNow;
            review.Deadline = request.Review.Deadline ?? DateTime.UtcNow.AddDays(14);
            review.IsCompleted = true;
            review.CompletedDate = DateTime.UtcNow;
            review.CreatedBy = request.ReviewerId;

            await _reviewRepository.AddAsync(review);

            // تحديث حالة البحث بناءً على عدد المراجعات المكتملة
            var research = await _researchRepository.GetByIdAsync(request.Review.ResearchId);
            if (research != null)
            {
                await UpdateResearchStatus(research, request.Review.ResearchId);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // إرسال إشعارات
            try
            {
                await _emailService.SendReviewCompletedNotificationAsync(review.Id);
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ لكن لا توقف العملية
                // يمكن إضافة logging هنا
            }

            return review.Id;
        }

        private async Task UpdateResearchStatus(Domain.Entities.Research research, int researchId)
        {
            var completedReviewsCount = await _reviewRepository.GetCompletedReviewsCountAsync(researchId);

            if (completedReviewsCount >= 3)
            {
                research.Status = Domain.Enums.ResearchStatus.UnderEvaluation;
                research.UpdatedAt = DateTime.UtcNow;
            }
            else if (research.Status == Domain.Enums.ResearchStatus.Submitted)
            {
                research.Status = Domain.Enums.ResearchStatus.UnderReview;
                research.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
