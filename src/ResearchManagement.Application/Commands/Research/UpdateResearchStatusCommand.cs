using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Application.Commands.Research
{
    public class UpdateResearchStatusCommand : IRequest<bool>
    {
        public int ResearchId { get; set; }
        public ResearchStatus NewStatus { get; set; }
        public string? Notes { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public class UpdateResearchStatusCommandHandler : IRequestHandler<UpdateResearchStatusCommand, bool>
    {
        private readonly IResearchRepository _researchRepository;
        private readonly IResearchStatusHistoryRepository _statusHistoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public UpdateResearchStatusCommandHandler(
            IResearchRepository researchRepository,
            IResearchStatusHistoryRepository statusHistoryRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService)
        {
            _researchRepository = researchRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<bool> Handle(UpdateResearchStatusCommand request, CancellationToken cancellationToken)
        {
            var research = await _researchRepository.GetByIdAsync(request.ResearchId);
            if (research == null) return false;

            var oldStatus = research.Status;
            research.Status = request.NewStatus;
            research.UpdatedAt = DateTime.UtcNow;
            research.UpdatedBy = request.UserId;

            // إضافة السجل للتاريخ
            var statusHistory = new Domain.Entities.ResearchStatusHistory
            {
                ResearchId = request.ResearchId,
                FromStatus = oldStatus,
                ToStatus = request.NewStatus,
                Notes = request.Notes,
                ChangedById = request.UserId,
                ChangedAt = DateTime.UtcNow
            };

            await _statusHistoryRepository.AddAsync(statusHistory);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // إرسال إشعار بالتحديث
            await _emailService.SendResearchStatusUpdateAsync(request.ResearchId, oldStatus, request.NewStatus);

            return true;
        }
    }
}
