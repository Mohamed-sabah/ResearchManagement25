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
    public class GetReviewByResearchAndReviewerQuery : IRequest<ReviewDto?>
{
    public int ResearchId { get; set; }
    public string ReviewerId { get; set; } = string.Empty;

    public GetReviewByResearchAndReviewerQuery(int researchId, string reviewerId)
    {
        ResearchId = researchId;
        ReviewerId = reviewerId;
    }
}

public class GetReviewByResearchAndReviewerQueryHandler : IRequestHandler<GetReviewByResearchAndReviewerQuery, ReviewDto?>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;

    public GetReviewByResearchAndReviewerQueryHandler(IReviewRepository reviewRepository, IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
    }

    public async Task<ReviewDto?> Handle(GetReviewByResearchAndReviewerQuery request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByResearchAndReviewerAsync(request.ResearchId, request.ReviewerId);
        return review != null ? _mapper.Map<ReviewDto>(review) : null;
    }
}

public class GetResearchFileByIdQuery : IRequest<ResearchFileDto?>
{
    public int FileId { get; set; }
    public string UserId { get; set; } = string.Empty;

    public GetResearchFileByIdQuery(int fileId, string userId)
    {
        FileId = fileId;
        UserId = userId;
    }
}

public class GetResearchFileByIdQueryHandler : IRequestHandler<GetResearchFileByIdQuery, ResearchFileDto?>
{
    private readonly IResearchFileRepository _fileRepository;
    private readonly IMapper _mapper;

    public GetResearchFileByIdQueryHandler(IResearchFileRepository fileRepository, IMapper mapper)
    {
        _fileRepository = fileRepository;
        _mapper = mapper;
    }

    public async Task<ResearchFileDto?> Handle(GetResearchFileByIdQuery request, CancellationToken cancellationToken)
    {
        var file = await _fileRepository.GetByIdWithAccessCheckAsync(request.FileId, request.UserId);
        return file != null ? _mapper.Map<ResearchFileDto>(file) : null;
    }
}
}