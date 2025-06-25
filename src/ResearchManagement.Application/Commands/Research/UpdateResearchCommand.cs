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
                // بدء Transaction
                await _unitOfWork.BeginTransactionAsync();

                // 1. جلب البحث الحالي مع التفاصيل
                var existingResearch = await _unitOfWork.Research.GetByIdWithDetailsAsync(request.ResearchId);
                if (existingResearch == null)
                {
                    _logger.LogWarning("البحث {ResearchId} غير موجود", request.ResearchId);
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                _logger.LogInformation("تم العثور على البحث: {Title}", existingResearch.Title);

                // 2. التحقق من صلاحية التعديل
                if (!CanEditResearch(existingResearch, request.UserId))
                {
                    _logger.LogWarning("المستخدم {UserId} لا يملك صلاحية تعديل البحث {ResearchId}",
                        request.UserId, request.ResearchId);
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                var originalStatus = existingResearch.Status;

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

                // 5. حفظ تغييرات البحث الأساسية أولاً
                await _unitOfWork.Research.UpdateAsync(existingResearch);
                var saveResult1 = await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("تم حفظ البيانات الأساسية - النتيجة: {Result}", saveResult1);

                // 6. تحديث المؤلفين إذا تم تمريرهم
                if (request.Research.Authors?.Any() == true)
                {
                    _logger.LogInformation("بدء تحديث المؤلفين - العدد: {Count}", request.Research.Authors.Count);

                    // إخفاء المؤلفين الحاليين (Soft Delete)
                    foreach (var existingAuthor in existingResearch.Authors.Where(a => !a.IsDeleted))
                    {
                        existingAuthor.IsDeleted = true;
                        existingAuthor.UpdatedAt = DateTime.UtcNow;
                        existingAuthor.UpdatedBy = request.UserId;
                    }

                    // إضافة المؤلفين الجدد
                    foreach (var authorDto in request.Research.Authors)
                    {
                        var author = _mapper.Map<Domain.Entities.ResearchAuthor>(authorDto);
                        author.ResearchId = existingResearch.Id;
                        author.CreatedAt = DateTime.UtcNow;
                        author.CreatedBy = request.UserId;
                        author.IsDeleted = false;

                        existingResearch.Authors.Add(author);
                        _logger.LogInformation("تم إضافة مؤلف جديد: {AuthorName}", author.FirstName);
                    }

                    var saveResult2 = await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("تم حفظ المؤلفين - النتيجة: {Result}", saveResult2);
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

                        existingResearch.Files.Add(fileEntity);
                        _logger.LogInformation("تم إضافة ملف جديد: {FileName}", fileEntity.OriginalFileName);
                    }

                    var saveResult3 = await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("تم حفظ الملفات - النتيجة: {Result}", saveResult3);
                }

                // 8. إرسال إشعار بالتحديث (بشكل منفصل عن Transaction)
                var shouldSendNotification = existingResearch.Status != originalStatus;

                // إتمام Transaction
                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("تم إتمام Transaction بنجاح");

                // إرسال الإشعار خارج Transaction
                if (shouldSendNotification)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل في تحديث البحث {ResearchId}: {ErrorMessage}",
                    request.ResearchId, ex.Message);

                try
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogInformation("تم إرجاع Transaction");
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "فشل في إرجاع Transaction");
                }

                throw new InvalidOperationException($"فشل في تحديث البحث: {ex.Message}", ex);
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



//using AutoMapper;
//using MediatR;
//using Microsoft.Extensions.Logging;
//using ResearchManagement.Application.DTOs;
//using ResearchManagement.Application.Interfaces;
//using ResearchManagement.Domain.Enums;
//using System;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace ResearchManagement.Application.Commands.Research
//{
//    public class UpdateResearchCommand : IRequest<bool>
//    {
//        public int ResearchId { get; set; }
//        public CreateResearchDto Research { get; set; } = new();
//        public string UserId { get; set; } = string.Empty;
//    }

//    public class UpdateResearchCommandHandler : IRequestHandler<UpdateResearchCommand, bool>
//    {
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly IMapper _mapper;
//        private readonly ILogger<UpdateResearchCommandHandler> _logger;
//        private readonly IEmailService _emailService;

//        public UpdateResearchCommandHandler(
//            IUnitOfWork unitOfWork,
//            IMapper mapper,
//            ILogger<UpdateResearchCommandHandler> logger,
//            IEmailService emailService)
//        {
//            _unitOfWork = unitOfWork;
//            _mapper = mapper;
//            _logger = logger;
//            _emailService = emailService;
//        }

//        public async Task<bool> Handle(UpdateResearchCommand request, CancellationToken cancellationToken)
//        {
//            _logger.LogInformation("بدء تحديث البحث {ResearchId} للمستخدم {UserId}",
//                request.ResearchId, request.UserId);

//            try
//            {
//                await _unitOfWork.BeginTransactionAsync();

//                // 1. جلب البحث الحالي
//                var existingResearch = await _unitOfWork.Research.GetByIdWithDetailsAsync(request.ResearchId);
//                if (existingResearch == null)
//                {
//                    _logger.LogWarning("البحث {ResearchId} غير موجود", request.ResearchId);
//                    return false;
//                }

//                // 2. التحقق من صلاحية التعديل
//                if (!CanEditResearch(existingResearch, request.UserId))
//                {
//                    _logger.LogWarning("المستخدم {UserId} لا يملك صلاحية تعديل البحث {ResearchId}",
//                        request.UserId, request.ResearchId);
//                    return false;
//                }

//                // 3. تحديث البيانات الأساسية
//                existingResearch.Title = request.Research.Title;
//                existingResearch.TitleEn = request.Research.TitleEn;
//                existingResearch.AbstractAr = request.Research.AbstractAr;
//                existingResearch.AbstractEn = request.Research.AbstractEn;
//                existingResearch.Keywords = request.Research.Keywords;
//                existingResearch.KeywordsEn = request.Research.KeywordsEn;
//                existingResearch.ResearchType = request.Research.ResearchType;
//                existingResearch.Language = request.Research.Language;
//                existingResearch.Track = request.Research.Track;
//                existingResearch.Methodology = request.Research.Methodology;
//                existingResearch.UpdatedAt = DateTime.UtcNow;
//                existingResearch.UpdatedBy = request.UserId;

//                // 4. تحديث حالة البحث إذا كان يتطلب تعديلات
//                if (existingResearch.Status == ResearchStatus.RequiresMinorRevisions ||
//                    existingResearch.Status == ResearchStatus.RequiresMajorRevisions)
//                {
//                    existingResearch.Status = ResearchStatus.RevisionsSubmitted;
//                }

//                await _unitOfWork.Research.UpdateAsync(existingResearch);

//                // 5. تحديث المؤلفين
//                if (request.Research.Authors?.Any() == true)
//                {
//                    await UpdateResearchAuthors(existingResearch, request.Research.Authors, request.UserId);
//                }

//                // 6. معالجة الملفات الجديدة
//                if (request.Research.Files?.Any() == true)
//                {
//                    await AddNewFiles(existingResearch, request.Research.Files, request.UserId);
//                }

//                await _unitOfWork.SaveChangesAsync(cancellationToken);

//                // 7. إرسال إشعار بالتحديث
//                try
//                {
//                    if (existingResearch.Status == ResearchStatus.RevisionsSubmitted)
//                    {
//                        await _emailService.SendResearchStatusUpdateAsync(
//                            request.ResearchId,
//                            ResearchStatus.RequiresMinorRevisions, // أو RequiresMajorRevisions
//                            ResearchStatus.RevisionsSubmitted);
//                    }
//                }
//                catch (Exception emailEx)
//                {
//                    _logger.LogWarning(emailEx, "فشل في إرسال إشعار التحديث");
//                }

//                await _unitOfWork.CommitTransactionAsync();

//                _logger.LogInformation("تم تحديث البحث {ResearchId} بنجاح", request.ResearchId);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "فشل في تحديث البحث {ResearchId}", request.ResearchId);
//                await _unitOfWork.RollbackTransactionAsync();
//                throw new InvalidOperationException($"فشل في تحديث البحث: {ex.Message}", ex);
//            }
//        }

//        private static bool CanEditResearch(Domain.Entities.Research research, string userId)
//        {
//            // التحقق من ملكية البحث
//            if (research.SubmittedById != userId)
//                return false;

//            // التحقق من حالة البحث
//            return research.Status == ResearchStatus.Submitted ||
//                   research.Status == ResearchStatus.RequiresMinorRevisions ||
//                   research.Status == ResearchStatus.RequiresMajorRevisions;
//        }

//        private async Task UpdateResearchAuthors(
//            Domain.Entities.Research research,
//            List<CreateResearchAuthorDto> newAuthors,
//            string userId)
//        {
//            // إزالة المؤلفين الحاليين (soft delete)
//            foreach (var existingAuthor in research.Authors)
//            {
//                existingAuthor.IsDeleted = true;
//                existingAuthor.UpdatedAt = DateTime.UtcNow;
//                existingAuthor.UpdatedBy = userId;
//            }

//            // إضافة المؤلفين الجدد
//            foreach (var authorDto in newAuthors)
//            {
//                var author = _mapper.Map<Domain.Entities.ResearchAuthor>(authorDto);
//                author.ResearchId = research.Id;
//                author.CreatedAt = DateTime.UtcNow;
//                author.CreatedBy = userId;

//                research.Authors.Add(author);
//            }
//        }

//        private async Task AddNewFiles(
//            Domain.Entities.Research research,
//            List<ResearchFileDto> newFiles,
//            string userId)
//        {
//            foreach (var fileDto in newFiles)
//            {
//                var fileEntity = new Domain.Entities.ResearchFile
//                {
//                    ResearchId = research.Id,
//                    FileName = fileDto.FileName,
//                    OriginalFileName = fileDto.OriginalFileName,
//                    FilePath = fileDto.FilePath,
//                    ContentType = fileDto.ContentType,
//                    FileSize = fileDto.FileSize,
//                    FileType = fileDto.FileType,
//                    Description = fileDto.Description ?? "ملف محدث",
//                    Version = GetNextVersion(research.Files),
//                    IsActive = true,
//                    CreatedAt = DateTime.UtcNow,
//                    CreatedBy = userId
//                };

//                research.Files.Add(fileEntity);
//            }
//        }

//        private static int GetNextVersion(ICollection<Domain.Entities.ResearchFile> existingFiles)
//        {
//            return existingFiles?.Where(f => f.IsActive)?.Max(f => f.Version) + 1 ?? 1;
//        }
//    }
//}