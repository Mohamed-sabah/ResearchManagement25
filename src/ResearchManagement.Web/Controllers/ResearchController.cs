using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Commands.Research;
using ResearchManagement.Application.Interfaces;

namespace ResearchManagement.Web.Controllers
{
    [Authorize]
    public class ResearchController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IResearchRepository _researchRepository;
        private readonly IFileService _fileService;

        public ResearchController(
            UserManager<User> userManager,
            IMediator mediator,
            IResearchRepository researchRepository,
            IFileService fileService) : base(userManager)
        {
            _mediator = mediator;
            _researchRepository = researchRepository;
            _fileService = fileService;
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
                return View(model);

            try
            {
                var command = new CreateResearchCommand
                {
                    Research = model,
                    UserId = GetCurrentUserId()
                };

                var researchId = await _mediator.Send(command);

                // رفع الملف إذا تم تحديده
                if (researchFile != null && researchFile.Length > 0)
                {
                    await UploadResearchFile(researchId, researchFile, FileType.OriginalResearch);
                }

                AddSuccessMessage("تم تقديم البحث بنجاح");
                return RedirectToAction("Details", new { id = researchId });
            }
            catch (Exception ex)
            {
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
            // TODO: Implement file download logic
            return NotFound();
        }

        private async Task UploadResearchFile(int researchId, IFormFile file, FileType fileType)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            var fileContent = stream.ToArray();

            var fileName = await _fileService.UploadFileAsync(fileContent, file.FileName, file.ContentType);

            // TODO: Save file information to database
        }
    }
}
