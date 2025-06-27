using AutoMapper;
using MediatR;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchManagement.Application.Queries.TrackManager
{
    public class GetAvailableReviewersQuery : IRequest<List<ReviewerDto>>
    {
        public string TrackManagerId { get; set; } = string.Empty;
        public int ResearchId { get; set; }

        public GetAvailableReviewersQuery(string trackManagerId, int researchId)
        {
            TrackManagerId = trackManagerId;
            ResearchId = researchId;
        }
    }

    public class GetAvailableReviewersQueryHandler : IRequestHandler<GetAvailableReviewersQuery, List<ReviewerDto>>
    {
        private readonly ITrackManagerRepository _trackManagerRepository;
        private readonly ITrackReviewerRepository _trackReviewerRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IMapper _mapper;

        public GetAvailableReviewersQueryHandler(
            ITrackManagerRepository trackManagerRepository,
            ITrackReviewerRepository trackReviewerRepository,
            IReviewRepository reviewRepository,
            IMapper mapper)
        {
            _trackManagerRepository = trackManagerRepository;
            _trackReviewerRepository = trackReviewerRepository;
            _reviewRepository = reviewRepository;
            _mapper = mapper;
        }

        public async Task<List<ReviewerDto>> Handle(GetAvailableReviewersQuery request, CancellationToken cancellationToken)
        {
            var trackManager = await _trackManagerRepository.GetByUserIdAsync(request.TrackManagerId);
            if (trackManager == null)
                throw new ArgumentException("مدير المسار غير موجود");

            // جلب جميع مراجعي المسار
            var trackReviewers = await _trackReviewerRepository.GetByTrackAsync(trackManager.Track);

            // جلب المراجعين المعينين حالياً للبحث
            var assignedReviewers = await _reviewRepository.GetByResearchIdAsync(request.ResearchId);
            var assignedReviewerIds = assignedReviewers.Select(r => r.ReviewerId).ToHashSet();

            // تصفية المراجعين المتاحين
            var availableReviewers = trackReviewers
                .Where(tr => tr.IsActive &&
                            tr.Reviewer.IsActive &&
                !assignedReviewerIds.Contains(tr.ReviewerId))
                .Select(tr => tr.Reviewer)
                .ToList();

            return _mapper.Map<List<ReviewerDto>>(availableReviewers);
        }
    }
}