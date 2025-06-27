using AutoMapper;
using MediatR;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Application.Queries.Research;
using ResearchManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchManagement.Application.Queries.TrackManager
{
    public class GetTrackResearchesQuery : IRequest<PagedResult<ResearchDto>>
    {
        public string TrackManagerId { get; set; } = string.Empty;
        public string? SearchTerm { get; set; }
        public ResearchStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetTrackResearchesQueryHandler : IRequestHandler<GetTrackResearchesQuery, PagedResult<ResearchDto>>
    {
        private readonly ITrackManagerRepository _trackManagerRepository;
        private readonly IResearchRepository _researchRepository;
        private readonly IMapper _mapper;

        public GetTrackResearchesQueryHandler(
            ITrackManagerRepository trackManagerRepository,
            IResearchRepository researchRepository,
            IMapper mapper)
        {
            _trackManagerRepository = trackManagerRepository;
            _researchRepository = researchRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<ResearchDto>> Handle(GetTrackResearchesQuery request, CancellationToken cancellationToken)
        {
            var trackManager = await _trackManagerRepository.GetByUserIdAsync(request.TrackManagerId);
            if (trackManager == null)
                throw new ArgumentException("مدير المسار غير موجود");

            var researches = await _researchRepository.GetPagedByTrackAsync(
                track: trackManager.Track,
                searchTerm: request.SearchTerm,
                status: request.Status,
                fromDate: request.FromDate,
                toDate: request.ToDate,
                page: request.Page,
                pageSize: request.PageSize);

            return new PagedResult<ResearchDto>
            {
                Items = _mapper.Map<List<ResearchDto>>(researches.Items),
                TotalCount = researches.TotalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)researches.TotalCount / request.PageSize)
            };
        }
    }
}
