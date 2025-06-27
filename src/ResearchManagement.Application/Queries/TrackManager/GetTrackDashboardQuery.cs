using MediatR;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchManagement.Application.Queries.TrackManager
{
    public class GetTrackDashboardQuery : IRequest<TrackDashboardDto>
    {
        public string TrackManagerId { get; set; } = string.Empty;

        public GetTrackDashboardQuery(string trackManagerId)
        {
            TrackManagerId = trackManagerId;
        }
    }

    public class GetTrackDashboardQueryHandler : IRequestHandler<GetTrackDashboardQuery, TrackDashboardDto>
    {
        private readonly ITrackManagerRepository _trackManagerRepository;
        private readonly IResearchRepository _researchRepository;
        private readonly IReviewRepository _reviewRepository;

        public GetTrackDashboardQueryHandler(
            ITrackManagerRepository trackManagerRepository,
            IResearchRepository researchRepository,
            IReviewRepository reviewRepository)
        {
            _trackManagerRepository = trackManagerRepository;
            _researchRepository = researchRepository;
            _reviewRepository = reviewRepository;
        }

        public async Task<TrackDashboardDto> Handle(GetTrackDashboardQuery request, CancellationToken cancellationToken)
        {
            var trackManager = await _trackManagerRepository.GetByUserIdAsync(request.TrackManagerId);
            if (trackManager == null)
                throw new ArgumentException("مدير المسار غير موجود");

            var track = trackManager.Track;

            // إحصائيات البحوث
            var researches = await _researchRepository.GetByTrackAsync(track);
            var reviews = await _reviewRepository.GetByTrackAsync(track);

            // البحوث الحديثة
            var recentResearches = researches
                .OrderByDescending(r => r.SubmissionDate)
                .Take(10)
                .ToList();

            // البحوث المعلقة
            var pendingResearches = researches
                .Where(r => r.Status == ResearchStatus.Submitted ||
                           r.Status == ResearchStatus.AssignedForReview)
                .ToList();

            // البحوث التي تحتاج مراجعين
            var researchesNeedingReviewers = researches
                .Where(r => r.Status == ResearchStatus.Submitted &&
                           r.Reviews.Count < 3)
                .ToList();

            // المراجعات المتأخرة
            var overdueReviews = reviews
                .Where(r => !r.IsCompleted && r.Deadline < DateTime.UtcNow)
                .ToList();

            // حساب الإحصائيات
            var statistics = new TrackStatisticsDto
            {
                TotalResearches = researches.Count,
                SubmittedResearches = researches.Count(r => r.Status == ResearchStatus.Submitted),
                UnderReviewResearches = researches.Count(r => r.Status == ResearchStatus.UnderReview ||
                                                             r.Status == ResearchStatus.AssignedForReview),
                AcceptedResearches = researches.Count(r => r.Status == ResearchStatus.Accepted),
                RejectedResearches = researches.Count(r => r.Status == ResearchStatus.Rejected),
                RequiringRevisionResearches = researches.Count(r => r.Status == ResearchStatus.RequiresMinorRevisions ||
                                                                r.Status == ResearchStatus.RequiresMajorRevisions),
                AverageProcessingTime = researches.Where(r => r.DecisionDate.HasValue)
                    .DefaultIfEmpty()
                    .Average(r => r?.DecisionDate != null ?
                        (r.DecisionDate.Value - r.SubmissionDate).TotalDays : 0)
            };

            return new TrackDashboardDto
            {
                TrackName = GetTrackDisplayName(track),
                Track = track,
                Statistics = statistics,
                RecentResearches = recentResearches,
                PendingResearches = pendingResearches,
                ResearchesNeedingReviewers = researchesNeedingReviewers,
                OverdueReviews = overdueReviews
            };
        }

        private static string GetTrackDisplayName(ResearchTrack track) => track switch
        {
            ResearchTrack.InformationTechnology => "تقنية المعلومات",
            ResearchTrack.InformationSecurity => "أمن المعلومات",
            ResearchTrack.ArtificialIntelligence => "الذكاء الاصطناعي",
            ResearchTrack.DataScience => "علوم البيانات",
            ResearchTrack.SoftwareEngineering => "هندسة البرمجيات",
            ResearchTrack.NetworkingAndCommunications => "الشبكات والاتصالات",
            _ => track.ToString()
        };
    }
}