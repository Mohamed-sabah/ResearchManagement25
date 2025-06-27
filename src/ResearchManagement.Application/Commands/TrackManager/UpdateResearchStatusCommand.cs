using MediatR;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchManagement.Application.Commands.TrackManager
{
    public class UpdateResearchStatusCommand : IRequest<bool>
    {
        public int ResearchId { get; set; }
        public ResearchStatus NewStatus { get; set; }
        public string? Notes { get; set; }
        public string TrackManagerId { get; set; } = string.Empty;
    }

    public class UpdateResearchStatusCommandHandler : IRequestHandler<UpdateResearchStatusCommand, bool>
    {
        private readonly IResearchRepository _researchRepository;
        private readonly ITrackManagerRepository _trackManagerRepository;
        private readonly IResearchStatusHistoryRepository _statusHistoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public UpdateResearchStatusCommandHandler(
            IResearchRepository researchRepository,
            ITrackManagerRepository trackManagerRepository,
            IResearchStatusHistoryRepository statusHistoryRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService)
        {
            _researchRepository = researchRepository;
            _trackManagerRepository = trackManagerRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<bool> Handle(UpdateResearchStatusCommand request, CancellationToken cancellationToken)
        {
            // التحقق من البحث
            var research = await _researchRepository.GetByIdAsync(request.ResearchId);
            if (research == null)
                return false;

            // التحقق من الصلاحيات
            var trackManager = await _trackManagerRepository.GetByUserIdAndTrackAsync(
                request.TrackManagerId, research.Track);
            if (trackManager == null)
                throw new UnauthorizedAccessException("غير مصرح لك بتحديث حالة هذا البحث");

            var oldStatus = research.Status;

            // تحديث الحالة
            research.Status = request.NewStatus;
            research.UpdatedAt = DateTime.UtcNow;
            research.UpdatedBy = request.TrackManagerId;

            // إضافة تاريخ القرار للحالات النهائية
            if (IsDecisionStatus(request.NewStatus))
            {
                research.DecisionDate = DateTime.UtcNow;
                if (request.NewStatus == ResearchStatus.Rejected && !string.IsNullOrEmpty(request.Notes))
                {
                    research.RejectionReason = request.Notes;
                }
            }

            await _researchRepository.UpdateAsync(research);

            // إضافة سجل في تاريخ الحالات
            var statusHistory = new Domain.Entities.ResearchStatusHistory
            {
                ResearchId = request.ResearchId,
                FromStatus = oldStatus,
                ToStatus = request.NewStatus,
                Notes = request.Notes,
                ChangedById = request.TrackManagerId,
                ChangedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.TrackManagerId
            };

            await _statusHistoryRepository.AddAsync(statusHistory);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // إرسال إشعار للمؤلف
            _ = Task.Run(async () => {
                try
                {
                    await _emailService.SendResearchStatusUpdateAsync(request.ResearchId, oldStatus, request.NewStatus);
                }
                catch
                {
                    // تسجيل الخطأ
                }
            });

            return true;
        }

        private static bool IsDecisionStatus(ResearchStatus status) => status switch
        {
            ResearchStatus.Accepted => true,
            ResearchStatus.Rejected => true,
            ResearchStatus.RequiresMinorRevisions => true,
            ResearchStatus.RequiresMajorRevisions => true,
            _ => false
        };
    }
}
