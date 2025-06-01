using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Interfaces;

namespace ResearchManagement.Application.Commands.Research
{
    public class CreateResearchCommand : IRequest<int>
    {
        public CreateResearchDto Research { get; set; } = new();
        public string UserId { get; set; } = string.Empty;
    }

    public class CreateResearchCommandHandler : IRequestHandler<CreateResearchCommand, int>
    {
        private readonly IResearchRepository _researchRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public CreateResearchCommandHandler(
            IResearchRepository researchRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IMapper mapper)
        {
            _researchRepository = researchRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<int> Handle(CreateResearchCommand request, CancellationToken cancellationToken)
        {
            var research = _mapper.Map<Domain.Entities.Research>(request.Research);
            research.SubmittedById = request.UserId;
            research.Status = Domain.Enums.ResearchStatus.Submitted;
            research.SubmissionDate = DateTime.UtcNow;

            await _researchRepository.AddAsync(research);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // إرسال بريد إلكتروني للتأكيد
            await _emailService.SendResearchSubmissionConfirmationAsync(research.Id);

            return research.Id;
        }
    }
}
