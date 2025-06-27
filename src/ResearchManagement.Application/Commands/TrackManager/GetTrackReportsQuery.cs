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
    public class GetTrackReportsQuery : IRequest<TrackReportDto>
    {
        public string TrackManagerId { get; set; } = string.Empty;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public GetTrackReportsQuery(string trackManagerId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            TrackManagerId = trackManagerId;
            FromDate = fromDate;
            ToDate = toDate;
        }
    }

    public class GetTrackReportsQueryHandler : IRequestHandler<GetTrackReportsQuery, TrackReportDto>
    {
        private readonly ITrackManagerRepository _trackManagerRepository;
        private readonly IResearchRepository _researchRepository;
        private readonly IReviewRepository _reviewRepository;

        public GetTrackReportsQueryHandler(
            ITrackManagerRepository trackManagerRepository,
            IResearchRepository researchRepository,
            IReviewRepository reviewRepository)
        {
            _trackManagerRepository = trackManagerRepository;
            _researchRepository = researchRepository;
            _reviewRepository = reviewRepository;
        }

        public async Task<TrackReportDto> Handle(GetTrackReportsQuery request, CancellationToken cancellationToken)
        {
            var trackManager = await _trackManagerRepository.GetByUserIdAsync(request.TrackManagerId);
            if (trackManager == null)
                throw new ArgumentException("مدير المسار غير موجود");

            var fromDate = request.FromDate ?? DateTime.UtcNow.AddMonths(-12);
            var toDate = request.ToDate ?? DateTime.UtcNow;

            var researches = await _researchRepository.GetByTrackAndDateRangeAsync(
                trackManager.Track, fromDate, toDate);
            var reviews = await _reviewRepository.GetByTrackAndDateRangeAsync(
                trackManager.Track, fromDate, toDate);

            var report = new TrackReportDto
            {
                TrackName = GetTrackDisplayName(trackManager.Track),
                Track = trackManager.Track,
                ReportGeneratedAt = DateTime.UtcNow,
                FromDate = fromDate,
                ToDate = toDate,
                TotalResearches = researches.Count,

                // إحصائيات البحوث حسب الحالة
                ResearchesByStatus = researches.GroupBy(r => r.Status)
                    .ToDictionary(g => g.Key, g => g.Count()),

                // إحصائيات البحوث حسب الشهر
                ResearchesByMonth = researches
                    .GroupBy(r => new { r.SubmissionDate.Year, r.SubmissionDate.Month })
                    .ToDictionary(g => $"{g.Key.Year}-{g.Key.Month:00}", g => g.Count()),

                // متوسط وقت المراجعة
                AverageReviewTime = reviews.Where(r => r.IsCompleted && r.CompletedDate.HasValue)
                    .DefaultIfEmpty()
                    .Average(r => r?.CompletedDate != null ?
                        (r.CompletedDate.Value - r.AssignedDate).TotalDays : 0),

                // أداء المراجعين
                ReviewerPerformance = reviews.GroupBy(r => r.ReviewerId)
                    .Select(g => new ReviewerPerformanceDto
                    {
                        ReviewerId = g.Key,
                        ReviewerName = g.First().Reviewer.FirstName + " " + g.First().Reviewer.LastName,
                        TotalReviews = g.Count(),
                        CompletedReviews = g.Count(r => r.IsCompleted),
                        AverageScore = g.Where(r => r.IsCompleted).DefaultIfEmpty()
                            .Average(r => r?.OverallScore ?? 0),
                        AcceptanceRate = g.Where(r => r.IsCompleted).Any() ?
                            (double)g.Count(r => r.IsCompleted && (r.Decision == ReviewDecision.AcceptAsIs ||
                                                                 r.Decision == ReviewDecision.AcceptWithMinorRevisions)) /
                            g.Count(r => r.IsCompleted) * 100 : 0
                    }).ToList(),

                // إحصائيات القرارات
                DecisionStatistics = reviews.Where(r => r.IsCompleted)
                    .GroupBy(r => r.Decision)
                    .ToDictionary(g => g.Key, g => g.Count()),

                // توزيع النقاط
                ScoreDistribution = reviews.Where(r => r.IsCompleted)
                    .GroupBy(r => Math.Floor(r.OverallScore))
                    .ToDictionary(g => g.Key.ToString(), g => g.Count())
            };

            return report;
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