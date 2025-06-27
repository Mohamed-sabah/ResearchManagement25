using AutoMapper;
using MediatR;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchManagement.Application.Commands.Review
{
    public class UpdateReviewCommand : IRequest<bool>
    {
        public UpdateReviewDto Review { get; set; } = new();
        public string UserId { get; set; } = string.Empty;
    }

    public class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand, bool>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateReviewCommandHandler(
            IReviewRepository reviewRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _reviewRepository = reviewRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<bool> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
        {
            var review = await _reviewRepository.GetByIdAsync(request.Review.Id);
            if (review == null)
                return false;

            // التحقق من الصلاحيات
            if (review.ReviewerId != request.UserId)
                throw new UnauthorizedAccessException("غير مصرح لك بتعديل هذه المراجعة");

            // تحديث البيانات
            review.Decision = request.Review.Decision;
            review.OriginalityScore = request.Review.OriginalityScore;
            review.MethodologyScore = request.Review.MethodologyScore;
            review.ClarityScore = request.Review.ClarityScore;
            review.SignificanceScore = request.Review.SignificanceScore;
            review.ReferencesScore = request.Review.ReferencesScore;
            review.CommentsToAuthor = request.Review.CommentsToAuthor;
            review.CommentsToTrackManager = request.Review.CommentsToTrackManager;
            review.Recommendations = request.Review.Recommendations;
            review.RequiresReReview = request.Review.RequiresReReview;
            review.IsCompleted = request.Review.IsCompleted;
            review.CompletedDate = request.Review.IsCompleted ? DateTime.UtcNow : null;
            review.UpdatedAt = DateTime.UtcNow;
            review.UpdatedBy = request.UserId;

            await _reviewRepository.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}