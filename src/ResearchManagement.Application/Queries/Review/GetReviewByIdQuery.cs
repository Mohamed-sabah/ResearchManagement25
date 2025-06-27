using AutoMapper;
using MediatR;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchManagement.Application.Queries.Review
{
    public class GetReviewByIdQuery : IRequest<ReviewDto?>
    {
        public int ReviewId { get; set; }
        public string UserId { get; set; } = string.Empty;

        public GetReviewByIdQuery(int reviewId, string userId)
        {
            ReviewId = reviewId;
            UserId = userId;
        }
    }

    public class GetReviewByIdQueryHandler : IRequestHandler<GetReviewByIdQuery, ReviewDto?>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IMapper _mapper;

        public GetReviewByIdQueryHandler(IReviewRepository reviewRepository, IMapper mapper)
        {
            _reviewRepository = reviewRepository;
            _mapper = mapper;
        }

        public async Task<ReviewDto?> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
        {
            var review = await _reviewRepository.GetByIdWithDetailsAsync(request.ReviewId);

            if (review == null)
                return null;

            // التحقق من الصلاحيات
            if (!CanAccessReview(review, request.UserId))
                return null;

            return _mapper.Map<ReviewDto>(review);
        }

        private static bool CanAccessReview(Domain.Entities.Review review, string userId)
        {
            // المراجع يمكنه الوصول لمراجعاته
            if (review.ReviewerId == userId)
                return true;

            // مدير المسار يمكنه الوصول لمراجعات مساره
            if (review.Research.AssignedTrackManagerId.HasValue &&
                review.Research.AssignedTrackManager?.UserId == userId)
                return true;

            // المؤلف يمكنه الوصول للمراجعات المكتملة لبحثه
            if (review.Research.SubmittedById == userId && review.IsCompleted)
                return true;

            return false;
        }
    }
}