using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ResearchManagement.Application.Commands.Research
{
    public class UpdateResearchCommand : IRequest<bool>
    {
        public int ResearchId { get; set; }
        public CreateResearchDto Research { get; set; } = new();
        public string UserId { get; set; } = string.Empty;
    }

    public class UpdateResearchCommandHandler : IRequestHandler<UpdateResearchCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateResearchCommandHandler> _logger;
        private readonly IEmailService _emailService;

        public UpdateResearchCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UpdateResearchCommandHandler> logger,
            IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<bool> Handle(UpdateResearchCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("بدء تحديث البحث {ResearchId} للمستخدم {UserId}",
                request.ResearchId, request.UserId);

            try
            {
                // 1. جلب البحث الحالي مع التفاصيل
                var existingResearch = await _unitOfWork.Research.GetByIdWithDetailsAsync(request.ResearchId);
                if (existingResearch == null)
                {
                    _logger.LogWarning("البحث {ResearchId} غير موجود", request.ResearchId);
                    return false;
                }

                _logger.LogInformation("تم العثور على البحث: {Title}", existingResearch.Title);

                // 2. التحقق من صلاحية التعديل
                if (!CanEditResearch(existingResearch, request.UserId))
                {
                    _logger.LogWarning("المستخدم {UserId} لا يملك صلاحية تعديل البحث {ResearchId}",
                        request.UserId, request.ResearchId);
                    return false;
                }

                var originalStatus = existingResearch.Status;

                // بدء Transaction
                await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 3. تحديث البيانات الأساسية
                existingResearch.Title = request.Research.Title;
                    existingResearch.TitleEn = request.Research.TitleEn;
                    existingResearch.AbstractAr = request.Research.AbstractAr;
                    existingResearch.AbstractEn = request.Research.AbstractEn;
                    existingResearch.Keywords = request.Research.Keywords;
                    existingResearch.KeywordsEn = request.Research.KeywordsEn;
                    existingResearch.ResearchType = request.Research.ResearchType;
                    existingResearch.Language = request.Research.Language;
                    existingResearch.Track = request.Research.Track;
                    existingResearch.Methodology = request.Research.Methodology;
                    existingResearch.UpdatedAt = DateTime.UtcNow;
                    existingResearch.UpdatedBy = request.UserId;

                    _logger.LogInformation("تم تحديث البيانات الأساسية للبحث");

                    // 4. تحديث حالة البحث إذا كان يتطلب تعديلات
                    if (existingResearch.Status == ResearchStatus.RequiresMinorRevisions ||
                        existingResearch.Status == ResearchStatus.RequiresMajorRevisions)
                    {
                        existingResearch.Status = ResearchStatus.RevisionsSubmitted;
                        _logger.LogInformation("تم تغيير حالة البحث إلى: RevisionsSubmitted");
                    }

                    // 5. تحديث البحث
                    await _unitOfWork.Research.UpdateAsync(existingResearch);

                    // 6. تحديث المؤلفين إذا تم تمريرهم
                    if (request.Research.Authors?.Any() == true)
                    {
                        _logger.LogInformation("بدء تحديث المؤلفين - العدد: {Count}", request.Research.Authors.Count);

                        // إخفاء المؤلفين الحاليين (Soft Delete)
                        var currentAuthors = existingResearch.Authors.Where(a => !a.IsDeleted).ToList();
                        foreach (var existingAuthor in currentAuthors)
                        {
                            existingAuthor.IsDeleted = true;
                            existingAuthor.UpdatedAt = DateTime.UtcNow;
                            existingAuthor.UpdatedBy = request.UserId;
                            await _unitOfWork.ResearchAuthors.UpdateAsync(existingAuthor);
                        }

                        // إضافة المؤلفين الجدد
                        foreach (var authorDto in request.Research.Authors)
                        {
                            var author = _mapper.Map<Domain.Entities.ResearchAuthor>(authorDto);
                            author.ResearchId = existingResearch.Id;
                            author.CreatedAt = DateTime.UtcNow;
                            author.CreatedBy = request.UserId;
                            author.IsDeleted = false;

                            await _unitOfWork.ResearchAuthors.AddAsync(author);
                            _logger.LogInformation("تم إضافة مؤلف جديد: {AuthorName}", author.FirstName);
                        }
                    }

                    // 7. إضافة الملفات الجديدة إذا وُجدت
                    if (request.Research.Files?.Any() == true)
                    {
                        _logger.LogInformation("بدء إضافة الملفات الجديدة - العدد: {Count}", request.Research.Files.Count);

                        foreach (var fileDto in request.Research.Files)
                        {
                            var fileEntity = new Domain.Entities.ResearchFile
                            {
                                ResearchId = existingResearch.Id,
                                FileName = fileDto.FileName,
                                OriginalFileName = fileDto.OriginalFileName,
                                FilePath = fileDto.FilePath,
                                ContentType = fileDto.ContentType,
                                FileSize = fileDto.FileSize,
                                FileType = fileDto.FileType,
                                Description = fileDto.Description ?? "ملف محدث",
                                Version = GetNextVersion(existingResearch.Files),
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = request.UserId
                            };

                            await _unitOfWork.ResearchFiles.AddAsync(fileEntity);
                            _logger.LogInformation("تم إضافة ملف جديد: {FileName}", fileEntity.OriginalFileName);
                        }
                    }

                    // 8. حفظ جميع التغييرات دفعة واحدة
                    var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("تم حفظ جميع التغييرات - عدد السجلات المتأثرة: {Count}", saveResult);

                    // 9. إتمام Transaction
                    await _unitOfWork.CommitTransactionAsync();
                    _logger.LogInformation("تم إتمام Transaction بنجاح");

                    // 10. إرسال الإشعار خارج Transaction
                    if (existingResearch.Status != originalStatus)
                    {
                        try
                        {
                            await _emailService.SendResearchStatusUpdateAsync(
                                request.ResearchId,
                                originalStatus,
                                existingResearch.Status);
                            _logger.LogInformation("تم إرسال إشعار التحديث");
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogWarning(emailEx, "فشل في إرسال إشعار التحديث - سيتم تجاهل الخطأ");
                        }
                    }

                    _logger.LogInformation("تم تحديث البحث {ResearchId} بنجاح", request.ResearchId);
                    return true;
                }
            catch (Exception innerEx)
            {
                _logger.LogError(innerEx, "خطأ داخلي أثناء تحديث البحث {ResearchId}", request.ResearchId);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل في تحديث البحث {ResearchId}: {ErrorMessage}",
                    request.ResearchId, ex.Message);

                // تحسين رسالة الخطأ
                var detailedMessage = ex.InnerException != null
                    ? $"{ex.Message} - التفاصيل: {ex.InnerException.Message}"
                    : ex.Message;

                throw new InvalidOperationException($"فشل في تحديث البحث: {detailedMessage}", ex);
    }
}

private static bool CanEditResearch(Domain.Entities.Research research, string userId)
        {
            // التحقق من ملكية البحث
            if (research.SubmittedById != userId)
            {
                return false;
            }

            // التحقق من حالة البحث
            return research.Status == ResearchStatus.Submitted ||
                   research.Status == ResearchStatus.RequiresMinorRevisions ||
                   research.Status == ResearchStatus.RequiresMajorRevisions;
        }

        private static int GetNextVersion(ICollection<Domain.Entities.ResearchFile> existingFiles)
        {
            if (existingFiles == null || !existingFiles.Any())
                return 1;

            var activeFiles = existingFiles.Where(f => f.IsActive && !f.IsDeleted);
            return activeFiles.Any() ? activeFiles.Max(f => f.Version) + 1 : 1;
        }
    }
}