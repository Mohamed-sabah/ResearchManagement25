
using System;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Entities;
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



        //public async Task<bool> Handle(UpdateResearchCommand request, CancellationToken cancellationToken)
        //{
        //    _logger.LogInformation("بدء تحديث البحث {ResearchId} للمستخدم {UserId}",
        //        request.ResearchId, request.UserId);
        //    _logger.LogDebug("خطوة 1: جلب البحث مكتملة");
        //    _logger.LogDebug("خطوة 2: التحقق من الصلاحيات مكتمل");
        //    try
        //    {
        //        // 1. جلب البحث الحالي مع التفاصيل
        //        var existingResearch = await _unitOfWork.Research.GetByIdWithDetailsAsync(request.ResearchId);
        //        if (existingResearch == null)
        //        {
        //            _logger.LogWarning("البحث {ResearchId} غير موجود", request.ResearchId);
        //            return false;
        //        }

        //        // 2. التحقق من صلاحية التعديل
        //        if (!CanEditResearch(existingResearch, request.UserId))
        //        {
        //            _logger.LogWarning("المستخدم {UserId} لا يملك صلاحية تعديل البحث {ResearchId}",
        //                request.UserId, request.ResearchId);
        //            return false;
        //        }

        //        var originalStatus = existingResearch.Status;

        //        // بدء Transaction
        //        await _unitOfWork.BeginTransactionAsync();

        //        // 3. تحديث البيانات الأساسية
        //        existingResearch.Title = request.Research.Title;
        //        existingResearch.TitleEn = request.Research.TitleEn;
        //        existingResearch.AbstractAr = request.Research.AbstractAr;
        //        existingResearch.AbstractEn = request.Research.AbstractEn;
        //        existingResearch.Keywords = request.Research.Keywords;
        //        existingResearch.KeywordsEn = request.Research.KeywordsEn;
        //        existingResearch.ResearchType = request.Research.ResearchType;
        //        existingResearch.Language = request.Research.Language;
        //        existingResearch.Track = request.Research.Track;
        //        existingResearch.Methodology = request.Research.Methodology;
        //        existingResearch.UpdatedAt = DateTime.UtcNow;
        //        existingResearch.UpdatedBy = request.UserId;

        //        // 4. تحديث حالة البحث إذا كان يتطلب تعديلات
        //        if (existingResearch.Status == ResearchStatus.RequiresMinorRevisions ||
        //            existingResearch.Status == ResearchStatus.RequiresMajorRevisions)
        //        {
        //            existingResearch.Status = ResearchStatus.RevisionsSubmitted;
        //            _logger.LogInformation("تم تغيير حالة البحث إلى: RevisionsSubmitted");
        //        }

        //        // 5. تحديث البحث
        //        await _unitOfWork.Research.UpdateAsync(existingResearch);

        //        // 6. تحديث المؤلفين
        //        await UpdateAuthors(existingResearch, request.Research.Authors, request.UserId, cancellationToken);

        //        // 7. إضافة الملفات الجديدة
        //        await AddNewFiles(existingResearch, request.Research.Files, request.UserId, cancellationToken);

        //        // 8. حفظ جميع التغييرات
        //        var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);
        //        _logger.LogInformation("تم حفظ جميع التغييرات - عدد السجلات المتأثرة: {Count}", saveResult);

        //        // 9. إتمام Transaction
        //        await _unitOfWork.CommitTransactionAsync();

        //        // 10. إرسال الإشعار إذا تغيرت الحالة
        //        if (existingResearch.Status != originalStatus)
        //        {
        //            try
        //            {
        //                await _emailService.SendResearchStatusUpdateAsync(
        //                    request.ResearchId,
        //                    originalStatus,
        //                    existingResearch.Status);
        //            }
        //            catch (Exception emailEx)
        //            {
        //                _logger.LogWarning(emailEx, "فشل في إرسال إشعار التحديث");
        //            }
        //        }

        //        _logger.LogInformation("تم تحديث البحث {ResearchId} بنجاح", request.ResearchId);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "فشل في تحديث البحث {ResearchId}: {ErrorMessage}",
        //            request.ResearchId, ex.Message);

        //        try
        //        {
        //            await _unitOfWork.RollbackTransactionAsync();
        //        }
        //        catch (Exception rollbackEx)
        //        {
        //            _logger.LogError(rollbackEx, "فشل في إلغاء المعاملة");
        //        }

        //        throw new InvalidOperationException($"فشل في تحديث البحث: {ex.Message}", ex);
        //    }
        //}

        //private async Task UpdateAuthors(Domain.Entities.Research research, List<CreateResearchAuthorDto> newAuthors, string userId, CancellationToken cancellationToken)
        //{
        //    if (newAuthors?.Any() == true)
        //    {
        //        // إخفاء المؤلفين الحاليين (Soft Delete)
        //        var currentAuthors = research.Authors.Where(a => !a.IsDeleted).ToList();
        //        foreach (var existingAuthor in currentAuthors)
        //        {
        //            existingAuthor.IsDeleted = true;
        //            existingAuthor.UpdatedAt = DateTime.UtcNow;
        //            existingAuthor.UpdatedBy = userId;
        //            await _unitOfWork.ResearchAuthors.UpdateAsync(existingAuthor);
        //        }

        //        // إضافة المؤلفين الجدد
        //        foreach (var authorDto in newAuthors)
        //        {
        //            var author = _mapper.Map<ResearchAuthor>(authorDto);
        //            author.ResearchId = research.Id;
        //            author.CreatedAt = DateTime.UtcNow;
        //            author.CreatedBy = userId;
        //            author.IsDeleted = false;

        //            await _unitOfWork.ResearchAuthors.AddAsync(author);
        //        }
        //    }
        //}

        //private async Task AddNewFiles(Domain.Entities.Research research, List<ResearchFileDto> newFiles, string userId, CancellationToken cancellationToken)
        //{
        //    if (newFiles?.Any() == true)
        //    {
        //        foreach (var fileDto in newFiles)
        //        {
        //            var fileEntity = new ResearchFile
        //            {
        //                ResearchId = research.Id,
        //                FileName = fileDto.FileName,
        //                OriginalFileName = fileDto.OriginalFileName,
        //                FilePath = fileDto.FilePath,
        //                ContentType = fileDto.ContentType,
        //                FileSize = fileDto.FileSize,
        //                FileType = fileDto.FileType,
        //                Description = fileDto.Description ?? "ملف محدث",
        //                Version = GetNextVersion(research.Files),
        //                IsActive = true,
        //                CreatedAt = DateTime.UtcNow,
        //                CreatedBy = userId
        //            };

        //            await _unitOfWork.ResearchFiles.AddAsync(fileEntity);
        //        }
        //    }
        //}


        //public async Task<bool> Handle(UpdateResearchCommand request, CancellationToken cancellationToken)
        //{
        //    _logger.LogInformation("بدء تحديث البحث {ResearchId} للمستخدم {UserId}",
        //        request.ResearchId, request.UserId);

        //    try
        //    {
        //        // 1. جلب البحث الحالي مع التفاصيل
        //        var existingResearch = await _unitOfWork.Research.GetByIdWithDetailsAsync(request.ResearchId);
        //        if (existingResearch == null)
        //        {
        //            _logger.LogWarning("البحث {ResearchId} غير موجود", request.ResearchId);
        //            return false;
        //        }

        //        _logger.LogInformation("تم العثور على البحث: {Title}", existingResearch.Title);

        //        // 2. التحقق من صلاحية التعديل
        //        if (!CanEditResearch(existingResearch, request.UserId))
        //        {
        //            _logger.LogWarning("المستخدم {UserId} لا يملك صلاحية تعديل البحث {ResearchId}",
        //                request.UserId, request.ResearchId);
        //            return false;
        //        }

        //        var originalStatus = existingResearch.Status;

        //        // بدء Transaction
        //        await _unitOfWork.BeginTransactionAsync();

        //        // 3. تحديث البيانات الأساسية
        //        _logger.LogInformation("تحديث البيانات الأساسية للبحث {ResearchId}", request.ResearchId);

        //        existingResearch.Title = request.Research.Title;
        //        existingResearch.TitleEn = request.Research.TitleEn;
        //        existingResearch.AbstractAr = request.Research.AbstractAr;
        //        existingResearch.AbstractEn = request.Research.AbstractEn;
        //        existingResearch.Keywords = request.Research.Keywords;
        //        existingResearch.KeywordsEn = request.Research.KeywordsEn;
        //        existingResearch.ResearchType = request.Research.ResearchType;
        //        existingResearch.Language = request.Research.Language;
        //        existingResearch.Track = request.Research.Track;
        //        existingResearch.Methodology = request.Research.Methodology;
        //        existingResearch.UpdatedAt = DateTime.UtcNow;
        //        existingResearch.UpdatedBy = request.UserId;

        //        // 4. تحديث حالة البحث إذا كان يتطلب تعديلات
        //        if (existingResearch.Status == ResearchStatus.RequiresMinorRevisions ||
        //            existingResearch.Status == ResearchStatus.RequiresMajorRevisions)
        //        {
        //            existingResearch.Status = ResearchStatus.RevisionsSubmitted;
        //            _logger.LogInformation("تم تغيير حالة البحث إلى: RevisionsSubmitted");
        //        }

        //        // 5. تحديث البحث في قاعدة البيانات
        //        _logger.LogInformation("حفظ تحديثات البحث الأساسية");
        //        await _unitOfWork.Research.UpdateAsync(existingResearch);

        //        // حفظ التغييرات الأساسية أولاً
        //        var saveResult1 = await _unitOfWork.SaveChangesAsync(cancellationToken);
        //        _logger.LogInformation("تم حفظ التحديثات الأساسية - النتيجة: {Result}", saveResult1);

        //        // 6. تحديث المؤلفين
        //        if (request.Research.Authors?.Any() == true)
        //        {
        //            _logger.LogInformation("بدء تحديث المؤلفين - العدد: {Count}", request.Research.Authors.Count);
        //            await UpdateAuthors(existingResearch, request.Research.Authors, request.UserId, cancellationToken);

        //            var saveResult2 = await _unitOfWork.SaveChangesAsync(cancellationToken);
        //            _logger.LogInformation("تم حفظ المؤلفين - النتيجة: {Result}", saveResult2);
        //        }

        //        // 7. إضافة الملفات الجديدة
        //        if (request.Research.Files?.Any() == true)
        //        {
        //            _logger.LogInformation("بدء إضافة الملفات الجديدة - العدد: {Count}", request.Research.Files.Count);
        //            await AddNewFiles(existingResearch, request.Research.Files, request.UserId, cancellationToken);

        //            var saveResult3 = await _unitOfWork.SaveChangesAsync(cancellationToken);
        //            _logger.LogInformation("تم حفظ الملفات - النتيجة: {Result}", saveResult3);
        //        }

        //        // 8. إتمام Transaction
        //        await _unitOfWork.CommitTransactionAsync();

        //        // 9. إرسال الإشعار إذا تغيرت الحالة
        //        if (existingResearch.Status != originalStatus)
        //        {
        //            try
        //            {
        //                await _emailService.SendResearchStatusUpdateAsync(
        //                    request.ResearchId,
        //                    originalStatus,
        //                    existingResearch.Status);
        //                _logger.LogInformation("تم إرسال إشعار تحديث الحالة");
        //            }
        //            catch (Exception emailEx)
        //            {
        //                _logger.LogWarning(emailEx, "فشل في إرسال إشعار التحديث");
        //            }
        //        }

        //        _logger.LogInformation("تم تحديث البحث {ResearchId} بنجاح", request.ResearchId);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "فشل في تحديث البحث {ResearchId}: {ErrorMessage}",
        //            request.ResearchId, ex.Message);

        //        // طباعة Stack Trace كاملاً
        //        _logger.LogError("Stack Trace: {StackTrace}", ex.StackTrace);

        //        if (ex.InnerException != null)
        //        {
        //            _logger.LogError("Inner Exception: {InnerMessage}", ex.InnerException.Message);
        //            _logger.LogError("Inner Stack Trace: {InnerStackTrace}", ex.InnerException.StackTrace);
        //        }

        //        try
        //        {
        //            await _unitOfWork.RollbackTransactionAsync();
        //        }
        //        catch (Exception rollbackEx)
        //        {
        //            _logger.LogError(rollbackEx, "فشل في إلغاء المعاملة");
        //        }

        //        throw new InvalidOperationException($"فشل في تحديث البحث: {ex.Message}", ex);
        //    }
        //}

        //private async Task UpdateAuthors(Domain.Entities.Research research, List<CreateResearchAuthorDto> newAuthors, string userId, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (newAuthors?.Any() == true)
        //        {
        //            _logger.LogInformation("بدء تحديث المؤلفين للبحث {ResearchId}", research.Id);

        //            // إخفاء المؤلفين الحاليين (Soft Delete)
        //            var currentAuthors = research.Authors.Where(a => !a.IsDeleted).ToList();
        //            _logger.LogInformation("عدد المؤلفين الحاليين: {Count}", currentAuthors.Count);

        //            foreach (var existingAuthor in currentAuthors)
        //            {
        //                existingAuthor.IsDeleted = true;
        //                existingAuthor.UpdatedAt = DateTime.UtcNow;
        //                existingAuthor.UpdatedBy = userId;
        //                await _unitOfWork.ResearchAuthors.UpdateAsync(existingAuthor);
        //                _logger.LogInformation("تم وضع علامة حذف على المؤلف: {AuthorName}", $"{existingAuthor.FirstName} {existingAuthor.LastName}");
        //            }

        //            // إضافة المؤلفين الجدد
        //            foreach (var authorDto in newAuthors)
        //            {
        //                var author = _mapper.Map<ResearchAuthor>(authorDto);
        //                author.ResearchId = research.Id;
        //                author.CreatedAt = DateTime.UtcNow;
        //                author.CreatedBy = userId;
        //                author.IsDeleted = false;

        //                await _unitOfWork.ResearchAuthors.AddAsync(author);
        //                _logger.LogInformation("تم إضافة مؤلف جديد: {AuthorName}", $"{author.FirstName} {author.LastName}");
        //            }

        //            _logger.LogInformation("انتهى تحديث المؤلفين للبحث {ResearchId}", research.Id);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "خطأ في تحديث المؤلفين للبحث {ResearchId}", research.Id);
        //        throw;
        //    }
        //}

        //private async Task AddNewFiles(Domain.Entities.Research research, List<ResearchFileDto> newFiles, string userId, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (newFiles?.Any() == true)
        //        {
        //            _logger.LogInformation("بدء إضافة الملفات الجديدة للبحث {ResearchId}", research.Id);

        //            foreach (var fileDto in newFiles)
        //            {
        //                var fileEntity = new ResearchFile
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

        //                await _unitOfWork.ResearchFiles.AddAsync(fileEntity);
        //                _logger.LogInformation("تم إضافة ملف جديد: {FileName}", fileEntity.OriginalFileName);
        //            }

        //            _logger.LogInformation("انتهى إضافة الملفات الجديدة للبحث {ResearchId}", research.Id);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "خطأ في إضافة الملفات للبحث {ResearchId}", research.Id);
        //        throw;
        //    }
        //}


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

                // 3. تحديث البيانات الأساسية
                _logger.LogInformation("تحديث البيانات الأساسية للبحث {ResearchId}", request.ResearchId);

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

                // 4. تحديث حالة البحث إذا كان يتطلب تعديلات
                if (existingResearch.Status == ResearchStatus.RequiresMinorRevisions ||
                    existingResearch.Status == ResearchStatus.RequiresMajorRevisions)
                {
                    existingResearch.Status = ResearchStatus.RevisionsSubmitted;
                    _logger.LogInformation("تم تغيير حالة البحث إلى: RevisionsSubmitted");
                }

                // 5. تحديث البحث في قاعدة البيانات
                _logger.LogInformation("حفظ تحديثات البحث الأساسية");

                try
                {
                    await _unitOfWork.Research.UpdateAsync(existingResearch);

                    // حفظ التغييرات الأساسية أولاً
                    var saveResult1 = await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("تم حفظ التحديثات الأساسية - النتيجة: {Result}", saveResult1);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "فشل في حفظ التحديثات الأساسية للبحث {ResearchId}", request.ResearchId);
                    throw;
                }

                // 6. تحديث المؤلفين
                if (request.Research.Authors?.Any() == true)
                {
                    _logger.LogInformation("بدء تحديث المؤلفين - العدد: {Count}", request.Research.Authors.Count);

                    try
                    {
                        await UpdateAuthors(existingResearch, request.Research.Authors, request.UserId, cancellationToken);

                        var saveResult2 = await _unitOfWork.SaveChangesAsync(cancellationToken);
                        _logger.LogInformation("تم حفظ المؤلفين - النتيجة: {Result}", saveResult2);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "فشل في تحديث المؤلفين للبحث {ResearchId}", request.ResearchId);
                        throw;
                    }
                }

                // 7. إضافة الملفات الجديدة
                if (request.Research.Files?.Any() == true)
                {
                    _logger.LogInformation("بدء إضافة الملفات الجديدة - العدد: {Count}", request.Research.Files.Count);

                    try
                    {
                        await AddNewFiles(existingResearch, request.Research.Files, request.UserId, cancellationToken);

                        var saveResult3 = await _unitOfWork.SaveChangesAsync(cancellationToken);
                        _logger.LogInformation("تم حفظ الملفات - النتيجة: {Result}", saveResult3);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "فشل في إضافة الملفات للبحث {ResearchId}", request.ResearchId);
                        throw;
                    }
                }

                // 8. إتمام Transaction
                await _unitOfWork.CommitTransactionAsync();

                // 9. إرسال الإشعار إذا تغيرت الحالة
                if (existingResearch.Status != originalStatus)
                {
                    try
                    {
                        await _emailService.SendResearchStatusUpdateAsync(
                            request.ResearchId,
                            originalStatus,
                            existingResearch.Status);
                        _logger.LogInformation("تم إرسال إشعار تحديث الحالة");
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogWarning(emailEx, "فشل في إرسال إشعار التحديث");
                    }
                }

                _logger.LogInformation("تم تحديث البحث {ResearchId} بنجاح", request.ResearchId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل في تحديث البحث {ResearchId}: {ErrorMessage}",
                    request.ResearchId, ex.Message);

                // طباعة Stack Trace كاملاً
                _logger.LogError("Stack Trace: {StackTrace}", ex.StackTrace);

                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception: {InnerMessage}", ex.InnerException.Message);
                    _logger.LogError("Inner Stack Trace: {InnerStackTrace}", ex.InnerException.StackTrace);
                }

                try
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "فشل في إلغاء المعاملة");
                }

                throw new InvalidOperationException($"فشل في تحديث البحث: {ex.Message}", ex);
            }
        }

        private async Task UpdateAuthors(Domain.Entities.Research research, List<CreateResearchAuthorDto> newAuthors, string userId, CancellationToken cancellationToken)
        {
            try
            {
                if (newAuthors?.Any() == true)
                {
                    _logger.LogInformation("بدء تحديث المؤلفين للبحث {ResearchId}", research.Id);

                    // التحقق من صحة بيانات المؤلفين الجدد
                    foreach (var authorDto in newAuthors)
                    {
                        if (string.IsNullOrWhiteSpace(authorDto.FirstName))
                        {
                            throw new ArgumentException($"الاسم الأول مطلوب للمؤلف رقم {authorDto.Order}");
                        }
                        if (string.IsNullOrWhiteSpace(authorDto.LastName))
                        {
                            throw new ArgumentException($"اسم العائلة مطلوب للمؤلف رقم {authorDto.Order}");
                        }
                        if (string.IsNullOrWhiteSpace(authorDto.Email))
                        {
                            throw new ArgumentException($"البريد الإلكتروني مطلوب للمؤلف رقم {authorDto.Order}");
                        }

                        _logger.LogInformation("مؤلف صحيح: {FirstName} {LastName} <{Email}>",
                            authorDto.FirstName, authorDto.LastName, authorDto.Email);
                    }

                    // حذف المؤلفين الحاليين فعلياً لتجنب مشكلة الفهرس الفريد
                    var currentAuthors = research.Authors.Where(a => !a.IsDeleted).ToList();
                    _logger.LogInformation("عدد المؤلفين الحاليين للحذف: {Count}", currentAuthors.Count);

                    foreach (var existingAuthor in currentAuthors)
                    {
                        _logger.LogInformation("حذف المؤلف فعلياً: ID={AuthorId}, Name={AuthorName}",
                            existingAuthor.Id, $"{existingAuthor.FirstName} {existingAuthor.LastName}");

                        // حذف فعلي بدلاً من Soft Delete
                        await _unitOfWork.ResearchAuthors.DeleteAsync(existingAuthor);
                    }

                    // إضافة المؤلفين الجدد
                    foreach (var authorDto in newAuthors)
                    {
                        var author = _mapper.Map<ResearchAuthor>(authorDto);

                        // التأكد من إعداد القيم المطلوبة
                        author.ResearchId = research.Id;
                        author.CreatedAt = DateTime.UtcNow;
                        author.CreatedBy = userId;
                        author.IsDeleted = false;

                        // التأكد من أن Order صحيح
                        if (author.Order <= 0)
                        {
                            author.Order = authorDto.Order > 0 ? authorDto.Order : 1;
                        }

                        _logger.LogInformation("إضافة مؤلف جديد: {AuthorName}, ResearchId={ResearchId}, Order={Order}",
                            $"{author.FirstName} {author.LastName}", author.ResearchId, author.Order);

                        await _unitOfWork.ResearchAuthors.AddAsync(author);
                    }

                    _logger.LogInformation("انتهى تحديث المؤلفين للبحث {ResearchId}", research.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث المؤلفين للبحث {ResearchId}", research.Id);
                throw;
            }
        }

        private async Task AddNewFiles(Domain.Entities.Research research, List<ResearchFileDto> newFiles, string userId, CancellationToken cancellationToken)
        {
            try
            {
                if (newFiles?.Any() == true)
                {
                    _logger.LogInformation("بدء إضافة الملفات الجديدة للبحث {ResearchId}", research.Id);

                    foreach (var fileDto in newFiles)
                    {
                        var fileEntity = new ResearchFile
                        {
                            ResearchId = research.Id,
                            FileName = fileDto.FileName,
                            OriginalFileName = fileDto.OriginalFileName,
                            FilePath = fileDto.FilePath,
                            ContentType = fileDto.ContentType,
                            FileSize = fileDto.FileSize,
                            FileType = fileDto.FileType,
                            Description = fileDto.Description ?? "ملف محدث",
                            Version = GetNextVersion(research.Files),
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = userId
                        };

                        await _unitOfWork.ResearchFiles.AddAsync(fileEntity);
                        _logger.LogInformation("تم إضافة ملف جديد: {FileName}", fileEntity.OriginalFileName);
                    }

                    _logger.LogInformation("انتهى إضافة الملفات الجديدة للبحث {ResearchId}", research.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إضافة الملفات للبحث {ResearchId}", research.Id);
                throw;
            }
        }
    

////        public async Task<bool> Handle(UpdateResearchCommand request, CancellationToken cancellationToken)
////        {
////            _logger.LogInformation("بدء تحديث البحث {ResearchId} للمستخدم {UserId}",
////                request.ResearchId, request.UserId);

////            try
////            {
////                // 1. جلب البحث الحالي مع التفاصيل
////                var existingResearch = await _unitOfWork.Research.GetByIdWithDetailsAsync(request.ResearchId);
////                if (existingResearch == null)
////                {
////                    _logger.LogWarning("البحث {ResearchId} غير موجود", request.ResearchId);
////                    return false;
////                }

////                _logger.LogInformation("تم العثور على البحث: {Title}", existingResearch.Title);

////                // 2. التحقق من صلاحية التعديل
////                if (!CanEditResearch(existingResearch, request.UserId))
////                {
////                    _logger.LogWarning("المستخدم {UserId} لا يملك صلاحية تعديل البحث {ResearchId}",
////                        request.UserId, request.ResearchId);
////                    return false;
////                }

////                var originalStatus = existingResearch.Status;

////                // بدء Transaction
////                await _unitOfWork.BeginTransactionAsync();

////                // 3. تحديث البيانات الأساسية
////                _logger.LogInformation("تحديث البيانات الأساسية للبحث {ResearchId}", request.ResearchId);

////                existingResearch.Title = request.Research.Title;
////                existingResearch.TitleEn = request.Research.TitleEn;
////                existingResearch.AbstractAr = request.Research.AbstractAr;
////                existingResearch.AbstractEn = request.Research.AbstractEn;
////                existingResearch.Keywords = request.Research.Keywords;
////                existingResearch.KeywordsEn = request.Research.KeywordsEn;
////                existingResearch.ResearchType = request.Research.ResearchType;
////                existingResearch.Language = request.Research.Language;
////                existingResearch.Track = request.Research.Track;
////                existingResearch.Methodology = request.Research.Methodology;
////                existingResearch.UpdatedAt = DateTime.UtcNow;
////                existingResearch.UpdatedBy = request.UserId;

////                // 4. تحديث حالة البحث إذا كان يتطلب تعديلات
////                if (existingResearch.Status == ResearchStatus.RequiresMinorRevisions ||
////                    existingResearch.Status == ResearchStatus.RequiresMajorRevisions)
////                {
////                    existingResearch.Status = ResearchStatus.RevisionsSubmitted;
////                    _logger.LogInformation("تم تغيير حالة البحث إلى: RevisionsSubmitted");
////                }

////                // 5. تحديث البحث في قاعدة البيانات
////                _logger.LogInformation("حفظ تحديثات البحث الأساسية");

////                try
////                {
////                    await _unitOfWork.Research.UpdateAsync(existingResearch);

////                    // حفظ التغييرات الأساسية أولاً
////                    var saveResult1 = await _unitOfWork.SaveChangesAsync(cancellationToken);
////                    _logger.LogInformation("تم حفظ التحديثات الأساسية - النتيجة: {Result}", saveResult1);
////                }
////                catch (Exception ex)
////                {
////                    _logger.LogError(ex, "فشل في حفظ التحديثات الأساسية للبحث {ResearchId}", request.ResearchId);
////                    throw;
////                }

////                // 6. تحديث المؤلفين
////                if (request.Research.Authors?.Any() == true)
////                {
////                    _logger.LogInformation("بدء تحديث المؤلفين - العدد: {Count}", request.Research.Authors.Count);

////                    try
////                    {
////                        await UpdateAuthors(existingResearch, request.Research.Authors, request.UserId, cancellationToken);

////                        var saveResult2 = await _unitOfWork.SaveChangesAsync(cancellationToken);
////                        _logger.LogInformation("تم حفظ المؤلفين - النتيجة: {Result}", saveResult2);
////                    }
////                    catch (Exception ex)
////                    {
////                        _logger.LogError(ex, "فشل في تحديث المؤلفين للبحث {ResearchId}", request.ResearchId);
////                        throw;
////                    }
////                }

////                // 7. إضافة الملفات الجديدة
////                if (request.Research.Files?.Any() == true)
////                {
////                    _logger.LogInformation("بدء إضافة الملفات الجديدة - العدد: {Count}", request.Research.Files.Count);

////                    try
////                    {
////                        await AddNewFiles(existingResearch, request.Research.Files, request.UserId, cancellationToken);

////                        var saveResult3 = await _unitOfWork.SaveChangesAsync(cancellationToken);
////                        _logger.LogInformation("تم حفظ الملفات - النتيجة: {Result}", saveResult3);
////                    }
////                    catch (Exception ex)
////                    {
////                        _logger.LogError(ex, "فشل في إضافة الملفات للبحث {ResearchId}", request.ResearchId);
////                        throw;
////                    }
////                }

////                // 8. إتمام Transaction
////                await _unitOfWork.CommitTransactionAsync();

////                // 9. إرسال الإشعار إذا تغيرت الحالة
////                if (existingResearch.Status != originalStatus)
////                {
////                    try
////                    {
////                        await _emailService.SendResearchStatusUpdateAsync(
////                            request.ResearchId,
////                            originalStatus,
////                            existingResearch.Status);
////                        _logger.LogInformation("تم إرسال إشعار تحديث الحالة");
////                    }
////                    catch (Exception emailEx)
////                    {
////                        _logger.LogWarning(emailEx, "فشل في إرسال إشعار التحديث");
////                    }
////                }

////                _logger.LogInformation("تم تحديث البحث {ResearchId} بنجاح", request.ResearchId);
////                return true;
////            }
////            catch (Exception ex)
////            {
////                _logger.LogError(ex, "فشل في تحديث البحث {ResearchId}: {ErrorMessage}",
////                    request.ResearchId, ex.Message);

////                // طباعة Stack Trace كاملاً
////                _logger.LogError("Stack Trace: {StackTrace}", ex.StackTrace);

////                if (ex.InnerException != null)
////                {
////                    _logger.LogError("Inner Exception: {InnerMessage}", ex.InnerException.Message);
////                    _logger.LogError("Inner Stack Trace: {InnerStackTrace}", ex.InnerException.StackTrace);
////                }

////                try
////                {
////                    await _unitOfWork.RollbackTransactionAsync();
////                }
////                catch (Exception rollbackEx)
////                {
////                    _logger.LogError(rollbackEx, "فشل في إلغاء المعاملة");
////                }

////                throw new InvalidOperationException($"فشل في تحديث البحث: {ex.Message}", ex);
////            }
////        }

////        private async Task UpdateAuthors(Domain.Entities.Research research, List<CreateResearchAuthorDto> newAuthors, string userId, CancellationToken cancellationToken)
////        {
////            try
////            {
////                if (newAuthors?.Any() == true)
////                {
////                    _logger.LogInformation("بدء تحديث المؤلفين للبحث {ResearchId}", research.Id);

////                    // التحقق من صحة بيانات المؤلفين الجدد
////                    foreach (var authorDto in newAuthors)
////                    {
////                        if (string.IsNullOrWhiteSpace(authorDto.FirstName))
////                        {
////                            throw new ArgumentException($"الاسم الأول مطلوب للمؤلف رقم {authorDto.Order}");
////                        }
////                        if (string.IsNullOrWhiteSpace(authorDto.LastName))
////                        {
////                            throw new ArgumentException($"اسم العائلة مطلوب للمؤلف رقم {authorDto.Order}");
////                        }
////                        if (string.IsNullOrWhiteSpace(authorDto.Email))
////                        {
////                            throw new ArgumentException($"البريد الإلكتروني مطلوب للمؤلف رقم {authorDto.Order}");
////                        }

////                        _logger.LogInformation("مؤلف صحيح: {FirstName} {LastName} <{Email}>",
////                            authorDto.FirstName, authorDto.LastName, authorDto.Email);
////                    }

////                    // إخفاء المؤلفين الحاليين (Soft Delete)
////                    var currentAuthors = research.Authors.Where(a => !a.IsDeleted).ToList();
////                    _logger.LogInformation("عدد المؤلفين الحاليين: {Count}", currentAuthors.Count);

////                    foreach (var existingAuthor in currentAuthors)
////                    {
////                        _logger.LogInformation("وضع علامة حذف على المؤلف: ID={AuthorId}, Name={AuthorName}",
////                            existingAuthor.Id, $"{existingAuthor.FirstName} {existingAuthor.LastName}");

////                        existingAuthor.IsDeleted = true;
////                        existingAuthor.UpdatedAt = DateTime.UtcNow;
////                        existingAuthor.UpdatedBy = userId;

////                        // التأكد من أن الكيان في حالة Modified
////                        await _unitOfWork.ResearchAuthors.UpdateAsync(existingAuthor);
////                    }

////                    // إضافة المؤلفين الجدد
////                    foreach (var authorDto in newAuthors)
////                    {
////                        var author = _mapper.Map<ResearchAuthor>(authorDto);

////                        // التأكد من إعداد القيم المطلوبة
////                        author.ResearchId = research.Id;
////                        author.CreatedAt = DateTime.UtcNow;
////                        author.CreatedBy = userId;
////                        author.IsDeleted = false;

////                        // التأكد من أن Order صحيح
////                        if (author.Order <= 0)
////                        {
////                            author.Order = authorDto.Order > 0 ? authorDto.Order : 1;
////                        }

////                        _logger.LogInformation("إضافة مؤلف جديد: {AuthorName}, ResearchId={ResearchId}, Order={Order}",
////                            $"{author.FirstName} {author.LastName}", author.ResearchId, author.Order);

////                        await _unitOfWork.ResearchAuthors.AddAsync(author);
////                    }

////                    _logger.LogInformation("انتهى تحديث المؤلفين للبحث {ResearchId}", research.Id);
////                }
////            }
////            catch (Exception ex)
////            {
////                _logger.LogError(ex, "خطأ في تحديث المؤلفين للبحث {ResearchId}", research.Id);
////                throw;
////            }
////        }

////        private async Task AddNewFiles(Domain.Entities.Research research, List<ResearchFileDto> newFiles, string userId, CancellationToken cancellationToken)
////        {
////            try
////            {
////                if (newFiles?.Any() == true)
////                {
////                    _logger.LogInformation("بدء إضافة الملفات الجديدة للبحث {ResearchId}", research.Id);

////                    foreach (var fileDto in newFiles)
////                    {
////                        var fileEntity = new ResearchFile
////                        {
////                            ResearchId = research.Id,
////                            FileName = fileDto.FileName,
////                            OriginalFileName = fileDto.OriginalFileName,
////                            FilePath = fileDto.FilePath,
////                            ContentType = fileDto.ContentType,
////                            FileSize = fileDto.FileSize,
////                            FileType = fileDto.FileType,
////                            Description = fileDto.Description ?? "ملف محدث",
////                            Version = GetNextVersion(research.Files),
////                            IsActive = true,
////                            CreatedAt = DateTime.UtcNow,
////                            CreatedBy = userId
////                        };

////                        await _unitOfWork.ResearchFiles.AddAsync(fileEntity);
////                        _logger.LogInformation("تم إضافة ملف جديد: {FileName}", fileEntity.OriginalFileName);
////                    }

////                    _logger.LogInformation("انتهى إضافة الملفات الجديدة للبحث {ResearchId}", research.Id);
////                }
////            }
////            catch (Exception ex)
////            {
////                _logger.LogError(ex, "خطأ في إضافة الملفات للبحث {ResearchId}", research.Id);
////                throw;
////            }
////        }


////        private static bool CanEditResearch(Domain.Entities.Research research, string userId)
////        {
////            // التحقق من ملكية البحث
////            if (research.SubmittedById != userId)
////            {
////                return false;
////            }

////            // التحقق من حالة البحث
////            return research.Status == ResearchStatus.Submitted ||
////                   research.Status == ResearchStatus.RequiresMinorRevisions ||
////                   research.Status == ResearchStatus.RequiresMajorRevisions;
////        }

////        private static int GetNextVersion(ICollection<Domain.Entities.ResearchFile> existingFiles)
////        {
////            if (existingFiles == null || !existingFiles.Any())
////                return 1;

////            var activeFiles = existingFiles.Where(f => f.IsActive && !f.IsDeleted);
////            return activeFiles.Any() ? activeFiles.Max(f => f.Version) + 1 : 1;
////        }
////    }

////}








//        public async Task<bool> Handle(UpdateResearchCommand request, CancellationToken cancellationToken)
//        {
//            _logger.LogInformation("بدء تحديث البحث {ResearchId} للمستخدم {UserId}",
//                request.ResearchId, request.UserId);

//            try
//            {
//                // 1. جلب البحث الحالي مع التفاصيل
//                var existingResearch = await _unitOfWork.Research.GetByIdWithDetailsAsync(request.ResearchId);
//                if (existingResearch == null)
//                {
//                    _logger.LogWarning("البحث {ResearchId} غير موجود", request.ResearchId);
//                    return false;
//                }

//                _logger.LogInformation("تم العثور على البحث: {Title}", existingResearch.Title);

//                // 2. التحقق من صلاحية التعديل
//                if (!CanEditResearch(existingResearch, request.UserId))
//                {
//                    _logger.LogWarning("المستخدم {UserId} لا يملك صلاحية تعديل البحث {ResearchId}",
//                        request.UserId, request.ResearchId);
//                    return false;
//                }

//                var originalStatus = existingResearch.Status;

//                // بدء Transaction
//                await _unitOfWork.BeginTransactionAsync();

//            try
//            {
//                // 3. تحديث البيانات الأساسية
//                existingResearch.Title = request.Research.Title;
//                    existingResearch.TitleEn = request.Research.TitleEn;
//                    existingResearch.AbstractAr = request.Research.AbstractAr;
//                    existingResearch.AbstractEn = request.Research.AbstractEn;
//                    existingResearch.Keywords = request.Research.Keywords;
//                    existingResearch.KeywordsEn = request.Research.KeywordsEn;
//                    existingResearch.ResearchType = request.Research.ResearchType;
//                    existingResearch.Language = request.Research.Language;
//                    existingResearch.Track = request.Research.Track;
//                    existingResearch.Methodology = request.Research.Methodology;
//                    existingResearch.UpdatedAt = DateTime.UtcNow;
//                    existingResearch.UpdatedBy = request.UserId;

//                    _logger.LogInformation("تم تحديث البيانات الأساسية للبحث");

//                    // 4. تحديث حالة البحث إذا كان يتطلب تعديلات
//                    if (existingResearch.Status == ResearchStatus.RequiresMinorRevisions ||
//                        existingResearch.Status == ResearchStatus.RequiresMajorRevisions)
//                    {
//                        existingResearch.Status = ResearchStatus.RevisionsSubmitted;
//                        _logger.LogInformation("تم تغيير حالة البحث إلى: RevisionsSubmitted");
//                    }

//                    // 5. تحديث البحث
//                    await _unitOfWork.Research.UpdateAsync(existingResearch);

//                    // 6. تحديث المؤلفين إذا تم تمريرهم
//                    if (request.Research.Authors?.Any() == true)
//                    {
//                        _logger.LogInformation("بدء تحديث المؤلفين - العدد: {Count}", request.Research.Authors.Count);

//                        // إخفاء المؤلفين الحاليين (Soft Delete)
//                        var currentAuthors = existingResearch.Authors.Where(a => !a.IsDeleted).ToList();
//                        foreach (var existingAuthor in currentAuthors)
//                        {
//                            existingAuthor.IsDeleted = true;
//                            existingAuthor.UpdatedAt = DateTime.UtcNow;
//                            existingAuthor.UpdatedBy = request.UserId;
//                            await _unitOfWork.ResearchAuthors.UpdateAsync(existingAuthor);
//                        }

//                        // إضافة المؤلفين الجدد
//                        foreach (var authorDto in request.Research.Authors)
//                        {
//                            var author = _mapper.Map<Domain.Entities.ResearchAuthor>(authorDto);
//                            author.ResearchId = existingResearch.Id;
//                            author.CreatedAt = DateTime.UtcNow;
//                            author.CreatedBy = request.UserId;
//                            author.IsDeleted = false;

//                            await _unitOfWork.ResearchAuthors.AddAsync(author);
//                            _logger.LogInformation("تم إضافة مؤلف جديد: {AuthorName}", author.FirstName);
//                        }
//                    }

//                    // 7. إضافة الملفات الجديدة إذا وُجدت
//                    if (request.Research.Files?.Any() == true)
//                    {
//                        _logger.LogInformation("بدء إضافة الملفات الجديدة - العدد: {Count}", request.Research.Files.Count);

//                        foreach (var fileDto in request.Research.Files)
//                        {
//                            var fileEntity = new Domain.Entities.ResearchFile
//                            {
//                                ResearchId = existingResearch.Id,
//                                FileName = fileDto.FileName,
//                                OriginalFileName = fileDto.OriginalFileName,
//                                FilePath = fileDto.FilePath,
//                                ContentType = fileDto.ContentType,
//                                FileSize = fileDto.FileSize,
//                                FileType = fileDto.FileType,
//                                Description = fileDto.Description ?? "ملف محدث",
//                                Version = GetNextVersion(existingResearch.Files),
//                                IsActive = true,
//                                CreatedAt = DateTime.UtcNow,
//                                CreatedBy = request.UserId
//                            };

//                            await _unitOfWork.ResearchFiles.AddAsync(fileEntity);
//                            _logger.LogInformation("تم إضافة ملف جديد: {FileName}", fileEntity.OriginalFileName);
//                        }
//                    }

//                    // 8. حفظ جميع التغييرات دفعة واحدة
//                    var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);
//                    _logger.LogInformation("تم حفظ جميع التغييرات - عدد السجلات المتأثرة: {Count}", saveResult);

//                    // 9. إتمام Transaction
//                    await _unitOfWork.CommitTransactionAsync();
//                    _logger.LogInformation("تم إتمام Transaction بنجاح");

//                    // 10. إرسال الإشعار خارج Transaction
//                    if (existingResearch.Status != originalStatus)
//                    {
//                        try
//                        {
//                            await _emailService.SendResearchStatusUpdateAsync(
//                                request.ResearchId,
//                                originalStatus,
//                                existingResearch.Status);
//                            _logger.LogInformation("تم إرسال إشعار التحديث");
//                        }
//                        catch (Exception emailEx)
//                        {
//                            _logger.LogWarning(emailEx, "فشل في إرسال إشعار التحديث - سيتم تجاهل الخطأ");
//                        }
//                    }

//                    _logger.LogInformation("تم تحديث البحث {ResearchId} بنجاح", request.ResearchId);
//                    return true;
//                }
//            catch (Exception innerEx)
//            {
//                _logger.LogError(innerEx, "خطأ داخلي أثناء تحديث البحث {ResearchId}", request.ResearchId);
//                await _unitOfWork.RollbackTransactionAsync();
//                throw;
//            }
//        }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "فشل في تحديث البحث {ResearchId}: {ErrorMessage}",
//                    request.ResearchId, ex.Message);

//                // تحسين رسالة الخطأ
//                var detailedMessage = ex.InnerException != null
//                    ? $"{ex.Message} - التفاصيل: {ex.InnerException.Message}"
//                    : ex.Message;

//                throw new InvalidOperationException($"فشل في تحديث البحث: {detailedMessage}", ex);
//    }
//}

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