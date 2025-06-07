using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Interfaces;

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
            var review = _mapper.Map<Domain.Entities.Review>(request.Review);
            review.ReviewerId = request.ReviewerId;
            review.CompletedDate = DateTime.UtcNow;
            review.IsCompleted = true;

            await _reviewRepository.AddAsync(review);

            // تحديث حالة البحث
            var research = await _researchRepository.GetByIdAsync(request.Review.ResearchId);
            if (research != null)
            {
                var completedReviews = await _reviewRepository.GetCompletedReviewsCountAsync(request.Review.ResearchId);

                if (completedReviews >= 3)
                {
                    research.Status = Domain.Enums.ResearchStatus.UnderEvaluation;
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // إرسال إشعار للباحث ومدير التراك
            await _emailService.SendReviewCompletedNotificationAsync(review.Id);

            return review.Id;
        }
    }
}
