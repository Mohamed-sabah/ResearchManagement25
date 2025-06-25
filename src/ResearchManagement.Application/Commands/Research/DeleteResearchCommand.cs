using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using Microsoft.Extensions.Logging;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResearchManagement.Application.Commands.Research
{
    public class DeleteResearchCommand : IRequest<bool>
    {
        public int ResearchId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public class DeleteResearchCommandHandler : IRequestHandler<DeleteResearchCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteResearchCommandHandler> _logger;
        private readonly IFileService _fileService;

        public DeleteResearchCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<DeleteResearchCommandHandler> logger,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _fileService = fileService;
        }

        public async Task<bool> Handle(DeleteResearchCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("بدء حذف البحث {ResearchId} بواسطة المستخدم {UserId}",
                request.ResearchId, request.UserId);

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 1. جلب البحث
                var research = await _unitOfWork.Research.GetByIdWithDetailsAsync(request.ResearchId);
                if (research == null)
                {
                    _logger.LogWarning("البحث {ResearchId} غير موجود", request.ResearchId);
                    return false;
                }

                // 2. التحقق من صلاحية الحذف
                if (!CanDeleteResearch(research, request.UserId))
                {
                    _logger.LogWarning("المستخدم {UserId} لا يملك صلاحية حذف البحث {ResearchId}",
                        request.UserId, request.ResearchId);
                    return false;
                }

                // 3. استخدام Soft Delete
                await _unitOfWork.Research.SoftDeleteAsync(request.ResearchId, request.UserId);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 4. حذف الملفات (اختياري - يمكن الاحتفاظ بها للأرشيف)
                // foreach (var file in research.Files.Where(f => f.IsActive))
                // {
                //     try
                //     {
                //         await _fileService.DeleteFileAsync(file.FilePath);
                //     }
                //     catch (Exception fileEx)
                //     {
                //         _logger.LogWarning(fileEx, "فشل في حذف الملف {FilePath}", file.FilePath);
                //     }
                // }

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("تم حذف البحث {ResearchId} بنجاح", request.ResearchId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل في حذف البحث {ResearchId}", request.ResearchId);
                await _unitOfWork.RollbackTransactionAsync();
                throw new InvalidOperationException($"فشل في حذف البحث: {ex.Message}", ex);
            }
        }

        private static bool CanDeleteResearch(Domain.Entities.Research research, string userId)
        {
            // التحقق من ملكية البحث
            if (research.SubmittedById != userId)
                return false;

            // يمكن حذف البحث فقط في حالة "مُقدم"
            return research.Status == ResearchStatus.Submitted;
        }
    }
}