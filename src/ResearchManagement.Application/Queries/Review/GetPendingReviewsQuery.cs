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
    public class GetPendingReviewsQuery : IRequest<List<ReviewDto>>
    {
        public string ReviewerId { get; set; } = string.Empty;

        public GetPendingReviewsQuery(string reviewerId)
        {
            ReviewerId = reviewerId;
        }
    }

    public class GetPendingReviewsQueryHandler : IRequestHandler<GetPendingReviewsQuery, List<ReviewDto>>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IMapper _mapper;

        public GetPendingReviewsQueryHandler(IReviewRepository reviewRepository, IMapper mapper)
        {
            _reviewRepository = reviewRepository;
            _mapper = mapper;
        }

        public async Task<List<ReviewDto>> Handle(GetPendingReviewsQuery request, CancellationToken cancellationToken)
        {
            var reviews = await _reviewRepository.GetPendingReviewsAsync(request.ReviewerId);
            return _mapper.Map<List<ReviewDto>>(reviews);
        }
    }
}