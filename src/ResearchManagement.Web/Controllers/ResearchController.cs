using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Commands.Research;
using ResearchManagement.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using ResearchManagement.Infrastructure.Data;

namespace ResearchManagement.Web.Controllers
{
    [Authorize]
    public class ResearchController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IResearchRepository _researchRepository;
        private readonly IFileService _fileService;
        private readonly ILogger<ResearchController> _logger;
        private readonly ApplicationDbContext _context;
        public ResearchController(
            UserManager<User> userManager,
            IMediator mediator,
            IResearchRepository researchRepository,
            IFileService fileService,
            ApplicationDbContext context,
            ILogger<ResearchController> logger) : base(userManager)
        {
            _mediator = mediator;
            _researchRepository = researchRepository;
            _fileService = fileService;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction("Login", "Account");

            IEnumerable<Research> researches;

            switch (user.Role)
            {
                case UserRole.Researcher:
                    researches = await _researchRepository.GetByUserIdAsync(user.Id);
                    break;
                case UserRole.ConferenceManager:
                    researches = await _researchRepository.GetAllAsync();
                    break;
                default:
                    researches = new List<Research>();
                    break;
            }

            return View(researches);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var research = await _researchRepository.GetByIdWithDetailsAsync(id);
            if (research == null)
                return NotFound();

            var user = await GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction("Login", "Account");

            // التحقق من الصلاحيات
            if (user.Role == UserRole.Researcher && research.SubmittedById != user.Id)
                return Forbid();

            return View(research);
        }

        [HttpGet]
        [Authorize(Roles = "Researcher")]
        public IActionResult Create()
        {
            var model = new CreateResearchDto();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Researcher")]
        public async Task<IActionResult> Create(CreateResearchDto model, IFormFile? researchFile)
        {
            if (!ModelState.IsValid)
            {
                // إضافة معلومات debug
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("Validation error: {Error}", error.ErrorMessage);
                }
                return View(model);
            }

            try
            {
                _logger.LogInformation("Creating research for user {UserId}: {Title}", GetCurrentUserId(), model.Title);

                var command = new CreateResearchCommand
                {
                    Research = model,
                    UserId = GetCurrentUserId()
                };

                var researchId = await _mediator.Send(command);

                _logger.LogInformation("Research created successfully with ID {ResearchId}", researchId);

                // رفع الملف إذا تم تحديده
                if (researchFile != null && researchFile.Length > 0)
                {
                    try
                    {
                        await UploadResearchFile(researchId, researchFile, FileType.OriginalResearch);
                        _logger.LogInformation("File uploaded successfully for research {ResearchId}", researchId);
                    }
                    catch (Exception fileEx)
                    {
                        _logger.LogError(fileEx, "Failed to upload file for research {ResearchId}", researchId);
                        AddWarningMessage("تم حفظ البحث بنجاح لكن فشل في رفع الملف");
                    }
                }

                AddSuccessMessage("تم تقديم البحث بنجاح");
                return RedirectToAction("Details", new { id = researchId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating research for user {UserId}", GetCurrentUserId());
                AddErrorMessage($"حدث خطأ في تقديم البحث: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Researcher")]
        public async Task<IActionResult> Edit(int id)
        {
            var research = await _researchRepository.GetByIdWithDetailsAsync(id);
            if (research == null)
                return NotFound();

            // التحقق من الملكية
            if (research.SubmittedById != GetCurrentUserId())
                return Forbid();

            // التحقق من إمكانية التعديل
            if (research.Status != ResearchStatus.Submitted &&
                research.Status != ResearchStatus.RequiresMinorRevisions &&
                research.Status != ResearchStatus.RequiresMajorRevisions)
            {
                AddWarningMessage("لا يمكن تعديل البحث في الحالة الحالية");
                return RedirectToAction("Details", new { id });
            }

            var model = new UpdateResearchDto
            {
                Id = research.Id,
                Title = research.Title,
                TitleEn = research.TitleEn,
                AbstractAr = research.AbstractAr,
                AbstractEn = research.AbstractEn,
                Keywords = research.Keywords,
                KeywordsEn = research.KeywordsEn,
                ResearchType = research.ResearchType,
                Language = research.Language,
                Track = research.Track,
                Methodology = research.Methodology,
                Authors = research.Authors.Select(a => new UpdateResearchAuthorDto
                {
                    Id = a.Id,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    FirstNameEn = a.FirstNameEn,
                    LastNameEn = a.LastNameEn,
                    Email = a.Email,
                    Institution = a.Institution,
                    AcademicDegree = a.AcademicDegree,
                    OrcidId = a.OrcidId,
                    Order = a.Order,
                    IsCorresponding = a.IsCorresponding
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Researcher")]
        public async Task<IActionResult> Edit(UpdateResearchDto model, IFormFile? newResearchFile)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var research = await _researchRepository.GetByIdAsync(model.Id);
                if (research == null)
                    return NotFound();

                // التحقق من الملكية
                if (research.SubmittedById != GetCurrentUserId())
                    return Forbid();

                // تحديث البيانات
                research.Title = model.Title;
                research.TitleEn = model.TitleEn;
                research.AbstractAr = model.AbstractAr;
                research.AbstractEn = model.AbstractEn;
                research.Keywords = model.Keywords;
                research.KeywordsEn = model.KeywordsEn;
                research.ResearchType = model.ResearchType;
                research.Language = model.Language;
                research.Track = model.Track;
                research.Methodology = model.Methodology;
                research.UpdatedAt = DateTime.UtcNow;
                research.UpdatedBy = GetCurrentUserId();

                // تحديث الحالة إذا كان البحث يتطلب تعديلات
                if (research.Status == ResearchStatus.RequiresMinorRevisions ||
                    research.Status == ResearchStatus.RequiresMajorRevisions)
                {
                    research.Status = ResearchStatus.RevisionsSubmitted;
                }

                await _researchRepository.UpdateAsync(research);

                // رفع ملف جديد إذا تم تحديده
                if (newResearchFile != null && newResearchFile.Length > 0)
                {
                    await UploadResearchFile(model.Id, newResearchFile, FileType.RevisedVersion);
                }

                AddSuccessMessage("تم تحديث البحث بنجاح");
                return RedirectToAction("Details", new { id = model.Id });
            }
            catch (Exception ex)
            {
                AddErrorMessage($"حدث خطأ في تحديث البحث: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadFile(int fileId)
        {
            try
            {
                // البحث عن الملف في قاعدة البيانات
                var researchFile = await _context.ResearchFiles
                    .Include(f => f.Research)
                    .FirstOrDefaultAsync(f => f.Id == fileId && f.IsActive);

                if (researchFile == null)
                {
                    AddErrorMessage("الملف غير موجود");
                    return NotFound();
                }

                // التحقق من صلاحيات الوصول
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // التحقق من الصلاحيات حسب دور المستخدم
                bool hasAccess = false;

                if (currentUser.Role == UserRole.SystemAdmin ||
                    currentUser.Role == UserRole.ConferenceManager)
                {
                    hasAccess = true; // المدراء لهم صلاحية كاملة
                }
                else if (currentUser.Role == UserRole.Researcher &&
                         researchFile.Research.SubmittedById == currentUser.Id)
                {
                    hasAccess = true; // الباحث يمكنه تحميل ملفات بحوثه
                }
                else if (currentUser.Role == UserRole.Reviewer)
                {
                    // المراجع يمكنه تحميل ملفات البحوث المكلف بمراجعتها
                    var hasReviewAccess = await _context.Reviews
                        .AnyAsync(r => r.ResearchId == researchFile.ResearchId &&
                                      r.ReviewerId == currentUser.Id);
                    hasAccess = hasReviewAccess;
                }
                else if (currentUser.Role == UserRole.TrackManager)
                {
                    // مدير التراك يمكنه تحميل ملفات بحوث تخصصه
                    var trackManager = await _context.TrackManagers
                        .FirstOrDefaultAsync(tm => tm.UserId == currentUser.Id &&
                                                  tm.Track == researchFile.Research.Track);
                    hasAccess = trackManager != null;
                }

                if (!hasAccess)
                {
                    AddErrorMessage("ليس لديك صلاحية لتحميل هذا الملف");
                    return Forbid();
                }

                // تحميل محتوى الملف
                var fileContent = await _fileService.DownloadFileAsync(researchFile.FilePath);

                // إرجاع الملف للتحميل
                return File(fileContent, researchFile.ContentType, researchFile.OriginalFileName);
            }
            catch (FileNotFoundException)
            {
                AddErrorMessage("الملف غير موجود على الخادم");
                return NotFound();
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ
                _logger?.LogError(ex, "Error downloading file {FileId}", fileId);
                AddErrorMessage("حدث خطأ أثناء تحميل الملف");
                return RedirectToAction("Index");
            }
        }

        private async Task UploadResearchFile(int researchId, IFormFile file, FileType fileType)
        {
            try
            {
                // التحقق من صحة الملف
                if (file == null || file.Length == 0)
                {
                    throw new ArgumentException("لم يتم تحديد ملف صالح");
                }

                // التحقق من نوع الملف
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    throw new ArgumentException($"نوع الملف غير مدعوم. الأنواع المدعومة: {string.Join(", ", allowedExtensions)}");
                }

                // التحقق من حجم الملف (50 ميجابايت)
                if (file.Length > 50 * 1024 * 1024)
                {
                    throw new ArgumentException("حجم الملف كبير جداً. الحد الأقصى 50 ميجابايت");
                }

                // تحويل الملف إلى byte array
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                var fileContent = stream.ToArray();

                // رفع الملف باستخدام FileService
                var filePath = await _fileService.UploadFileAsync(fileContent, file.FileName, file.ContentType);

                // حفظ معلومات الملف في قاعدة البيانات
                var researchFile = new ResearchFile
                {
                    ResearchId = researchId,
                    FileName = Path.GetFileName(filePath),
                    OriginalFileName = file.FileName,
                    FilePath = filePath,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    FileType = fileType,
                    Version = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = GetCurrentUserId()
                };

                _context.ResearchFiles.Add(researchFile);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error uploading file for research {ResearchId}", researchId);
                throw; // إعادة إرسال الخطأ للتعامل معه في المستوى الأعلى
            }
        }
    }
}
