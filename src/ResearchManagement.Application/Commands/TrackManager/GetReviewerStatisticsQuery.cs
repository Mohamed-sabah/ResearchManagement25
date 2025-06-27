using MediatR;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchManagement.Application.Queries.TrackManager
{
    public class GetReviewerStatisticsQuery : IRequest<List<ReviewerStatisticsDto>>
    {
        public string TrackManagerId { get; set; } = string.Empty;

        public GetReviewerStatisticsQuery(string trackManagerId)
        {
            TrackManagerId = trackManagerId;
        }
    }

    public class GetReviewerStatisticsQueryHandler : IRequestHandler<GetReviewerStatisticsQuery, List<ReviewerStatisticsDto>>
    {
        private readonly ITrackManagerRepository _trackManagerRepository;
        private readonly ITrackReviewerRepository _trackReviewerRepository;
        private readonly IReviewRepository _reviewRepository;

        public GetReviewerStatisticsQueryHandler(
            ITrackManagerRepository trackManagerRepository,
            ITrackReviewerRepository trackReviewerRepository,
            IReviewRepository reviewRepository)
        {
            _trackManagerRepository = trackManagerRepository;
            _trackReviewerRepository = trackReviewerRepository;
            _reviewRepository = reviewRepository;
        }

        public async Task<List<ReviewerStatisticsDto>> Handle(GetReviewerStatisticsQuery request, CancellationToken cancellationToken)
        {
            var trackManager = await _trackManagerRepository.GetByUserIdAsync(request.TrackManagerId);
            if (trackManager == null)
                throw new ArgumentException("مدير المسار غير موجود");

            var trackReviewers = await _trackReviewerRepository.GetByTrackAsync(trackManager.Track);
            var statistics = new List<ReviewerStatisticsDto>();

            foreach (var trackReviewer in trackReviewers)
            {
                var reviews = await _reviewRepository.GetByReviewerAndTrackAsync(
                    trackReviewer.ReviewerId, trackManager.Track);

                var completedReviews = reviews.Where(r => r.IsCompleted).ToList();
                var pendingReviews = reviews.Where(r => !r.IsCompleted).ToList();
                var overdueReviews = pendingReviews.Where(r => r.Deadline < DateTime.UtcNow).ToList();

                var stat = new ReviewerStatisticsDto
                {
                    ReviewerId = trackReviewer.ReviewerId,
                    ReviewerName = $"{trackReviewer.Reviewer.FirstName} {trackReviewer.Reviewer.LastName}",
                    ReviewerEmail = trackReviewer.Reviewer.Email,
                    TotalAssigned = reviews.Count,
                    CompletedReviews = completedReviews.Count,
                    PendingReviews = pendingReviews.Count,
                    OverdueReviews = overdueReviews.Count,
                    AverageReviewTime = completedReviews.Any() && completedReviews.All(r => r.CompletedDate.HasValue)
                        ? completedReviews.Average(r => (r.CompletedDate!.Value - r.AssignedDate).TotalDays)
                        : 0,
                    AverageScore = completedReviews.Any()
                        ? (double)completedReviews.Average(r => r.OverallScore)
                        : 0,
                    AcceptanceRate = completedReviews.Any()
                        ? (double)completedReviews.Count(r => r.Decision == ReviewDecision.AcceptAsIs ||
                                                            r.Decision == ReviewDecision.AcceptWithMinorRevisions) /
                          completedReviews.Count * 100
                        : 0,
                    IsActive = trackReviewer.IsActive,
                    LastReviewDate = completedReviews.OrderByDescending(r => r.CompletedDate)
                                                   .FirstOrDefault()?.CompletedDate
                };

                statistics.Add(stat);
            }

            return statistics.OrderByDescending(s => s.CompletedReviews)
                           .ThenBy(s => s.PendingReviews)
                           .ToList();
        }
    }
}
