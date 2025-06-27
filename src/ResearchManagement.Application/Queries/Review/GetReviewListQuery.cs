using AutoMapper;
using MediatR;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Application.Queries.Research;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchManagement.Application.Queries.Review
{
    public class GetReviewListQuery : IRequest<PagedResult<ReviewDto>>
    {
        public string UserId { get; set; } = string.Empty;
        public Domain.Enums.UserRole UserRole { get; set; }
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public string? Track { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetReviewListQueryHandler : IRequestHandler<GetReviewListQuery, PagedResult<ReviewDto>>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IMapper _mapper;

        public GetReviewListQueryHandler(IReviewRepository reviewRepository, IMapper mapper)
        {
            _reviewRepository = reviewRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<ReviewDto>> Handle(GetReviewListQuery request, CancellationToken cancellationToken)
        {
            var reviews = await _reviewRepository.GetPagedAsync(
                userId: request.UserId,
                userRole: request.UserRole,
                searchTerm: request.SearchTerm,
                status: request.Status,
                track: request.Track,
                fromDate: request.FromDate,
                toDate: request.ToDate,
                page: request.Page,
                pageSize: request.PageSize);

            return new PagedResult<ReviewDto>
            {
                Items = _mapper.Map<List<ReviewDto>>(reviews.Items),
                TotalCount = reviews.TotalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)reviews.TotalCount / request.PageSize)
            };
        }
    }
}