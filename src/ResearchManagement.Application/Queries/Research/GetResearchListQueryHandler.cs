using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Application.Queries.Research
{
    public class GetResearchListQueryHandler : IRequestHandler<GetResearchListQuery, PagedResult<ResearchDto>>
    {
        private readonly IResearchRepository _researchRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetResearchListQueryHandler> _logger;

        public GetResearchListQueryHandler(
            IResearchRepository researchRepository,
            IMapper mapper,
            ILogger<GetResearchListQueryHandler> logger)
        {
            _researchRepository = researchRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<ResearchDto>> Handle(GetResearchListQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("جاري جلب قائمة البحوث - الصفحة: {Page}, الحجم: {PageSize}", 
                request.Page, request.PageSize);

            // بناء معايير البحث
            var searchCriteria = BuildSearchCriteria(request);

            // جلب البيانات
            var (researches, totalCount) = await _researchRepository.GetPagedAsync(
                searchCriteria,
                request.Page,
                request.PageSize,
                request.SortBy ?? "SubmissionDate",
                request.SortDescending);

            // تحويل إلى DTOs
            var researchDtos = _mapper.Map<List<ResearchDto>>(researches);

            // تطبيق فلترة إضافية حسب دور المستخدم
            if (!string.IsNullOrEmpty(request.UserId) && request.UserRole.HasValue)
            {
                researchDtos = await FilterByUserRoleAsync(researchDtos, request.UserId, request.UserRole.Value);
            }

            var result = new PagedResult<ResearchDto>
            {
                Items = researchDtos,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };

            _logger.LogInformation("تم جلب {Count} بحث من أصل {Total}", 
                researchDtos.Count, totalCount);

            return result;
        }

        private Dictionary<string, object> BuildSearchCriteria(GetResearchListQuery request)
        {
            var criteria = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                criteria["SearchTerm"] = request.SearchTerm;

            if (request.Status.HasValue)
                criteria["Status"] = request.Status.Value;

            if (request.Track.HasValue)
                criteria["Track"] = request.Track.Value;

            if (request.FromDate.HasValue)
                criteria["FromDate"] = request.FromDate.Value;

            if (request.ToDate.HasValue)
                criteria["ToDate"] = request.ToDate.Value;

            return criteria;
        }

        private async Task<List<ResearchDto>> FilterByUserRoleAsync(
            List<ResearchDto> researches, 
            string userId, 
            UserRole userRole)
        {
            switch (userRole)
            {
                case UserRole.Researcher:
                    // الباحث يرى فقط البحوث التي هو مؤلف فيها
                    return researches.Where(r => 
                        r.SubmittedById == userId || 
                        r.Authors?.Any(a => a.UserId == userId) == true).ToList();

                case UserRole.Reviewer:
                    // المراجع يرى البحوث المعينة له للمراجعة
                    return researches.Where(r => 
                        r.Reviews?.Any(rev => rev.ReviewerId == userId) == true).ToList();

                case UserRole.TrackManager:
                    // مدير المسار يرى بحوث مساره فقط
                    return researches.Where(r => 
                        r.AssignedTrackManagerId == userId).ToList();

                case UserRole.Admin:
                    // الأدمن يرى كل شيء
                    return researches;

                default:
                    return new List<ResearchDto>();
            }
        }
    }
}