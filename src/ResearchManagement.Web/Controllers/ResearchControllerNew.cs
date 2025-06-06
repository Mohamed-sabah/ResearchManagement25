using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using AutoMapper;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Commands.Research;
using ResearchManagement.Application.Queries.Research;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Web.Models.ViewModels.Research;
using ResearchManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ResearchManagement.Web.Controllers
{
    [Authorize]
    public class ResearchControllerNew : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        private readonly ILogger<ResearchControllerNew> _logger;

        public ResearchControllerNew(
            UserManager<User> userManager,
            IMediator mediator,
            IMapper mapper,
            IFileService fileService,
            ILogger<ResearchControllerNew> logger) : base(userManager)
        {
            _mediator = mediator;
            _mapper = mapper;
            _fileService = fileService;
            _logger = logger;
        }

        // GET: Research
        public async Task<IActionResult> Index(
            string? searchTerm,
            ResearchStatus? status,
            ResearchTrack? track,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1,
            int pageSize = 10,
            string sortBy = "SubmissionDate",
            bool sortDescending = true)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var query = new GetResearchListQuery(user.Id, user.Role)
                {
                    SearchTerm = searchTerm,
                    Status = status,
                    Track = track,
                    FromDate = fromDate,
                    ToDate = toDate,
                    Page = page,
                    PageSize = pageSize,
                    SortBy = sortBy,
                    SortDescending = sortDescending
                };

                var result = await _mediator.Send(query);

                var viewModel = new ResearchListViewModel
                {
                    Researches = result,
                    Filter = new ResearchFilterViewModel
                    {
                        SearchTerm = searchTerm,
                        Status = status,
                        Track = track,
                        FromDate = fromDate,
                        ToDate = toDate,
                        Page = page,
                        PageSize = pageSize,
                        SortBy = sortBy,
                        SortDescending = sortDescending
                    },
                    StatusOptions = GetStatusOptions(),
                    TrackOptions = GetTrackOptions(),
                    CurrentUserId = user.Id,
                    CurrentUserRole = user.Role,
                    CanCreateResearch = CanCreateResearch(user.Role),
                    CanManageResearches = CanManageResearches(user.Role)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading research list");
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحميل قائمة البحوث";
                return View(new ResearchListViewModel());
            }
        }

        // GET: Research/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var query = new GetResearchByIdQuery(id, user.Id);
                var research = await _mediator.Send(query);

                if (research == null)
                {
                    TempData["ErrorMessage"] = "البحث غير موجود أو ليس لديك صلاحية للوصول إليه";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new ResearchDetailsViewModel
                {
                    Research = research,
                    Files = research.Files?.ToList() ?? new List<ResearchFileDto>(),
                    Reviews = research.Reviews?.ToList() ?? new List<ReviewDto>(),
                    CurrentUserId = user.Id,
                    CurrentUserRole = user.Role,
                    CanEdit = CanEditResearch(research, user),
                    CanDelete = CanDeleteResearch(research, user),
                    CanReview = CanReviewResearch(research, user),
                    CanManageStatus = CanManageStatus(user.Role),
                    CanDownloadFiles = CanDownloadFiles(research, user),
                    CanUploadFiles = CanUploadFiles(research, user),
                    IsAuthor = IsAuthor(research, user.Id),
                    IsReviewer = IsReviewer(research, user.Id),
                    IsTrackManager = IsTrackManager(research, user.Id)
                };

                // Calculate statistics
                if (viewModel.Reviews.Any())
                {
                    viewModel.TotalReviews = viewModel.Reviews.Count;
                    viewModel.CompletedReviews = viewModel.Reviews.Count(r => r.IsCompleted);
                    viewModel.PendingReviews = viewModel.TotalReviews - viewModel.CompletedReviews;
                    
                    var completedReviews = viewModel.Reviews.Where(r => r.IsCompleted && r.Score.HasValue);
                    if (completedReviews.Any())
                    {
                        viewModel.AverageScore = completedReviews.Average(r => r.Score!.Value);
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading research details for ID: {ResearchId}", id);
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحميل تفاصيل البحث";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Research/Create
        [Authorize(Roles = "Researcher,Admin")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var viewModel = new CreateResearchViewModel
                {
                    CurrentUserId = user.Id,
                    IsEditMode = false
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading create research page");
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحميل صفحة إنشاء البحث";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Research/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Researcher,Admin")]
        public async Task<IActionResult> Create(CreateResearchViewModel model, List<IFormFile> files)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = await GetCurrentUserAsync();
                if (user == null)
                    return RedirectToAction("Login", "Account");

                // Create research DTO
                var createResearchDto = _mapper.Map<CreateResearchDto>(model);
                
                // Handle file uploads
                if (files?.Any() == true)
                {
                    var uploadedFiles = new List<ResearchFileDto>();
                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            var fileResult = await _fileService.UploadFileAsync(file, "research");
                            if (fileResult.Success)
                            {
                                uploadedFiles.Add(new ResearchFileDto
                                {
                                    FileName = fileResult.FileName!,
                                    OriginalFileName = file.FileName,
                                    FilePath = fileResult.FilePath!,
                                    ContentType = file.ContentType,
                                    FileSize = file.Length,
                                    FileType = GetFileType(file.ContentType),
                                    Description = "ملف البحث الرئيسي"
                                });
                            }
                        }
                    }
                    createResearchDto.Files = uploadedFiles;
                }

                var command = new CreateResearchCommand
                {
                    Research = createResearchDto,
                    UserId = user.Id
                };

                var researchId = await _mediator.Send(command);

                TempData["SuccessMessage"] = "تم تقديم البحث بنجاح";
                return RedirectToAction(nameof(Details), new { id = researchId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating research");
                TempData["ErrorMessage"] = "حدث خطأ أثناء تقديم البحث";
                return View(model);
            }
        }

        // GET: Research/Edit/5
        [Authorize(Roles = "Researcher,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var query = new GetResearchByIdQuery(id, user.Id);
                var research = await _mediator.Send(query);

                if (research == null)
                {
                    TempData["ErrorMessage"] = "البحث غير موجود أو ليس لديك صلاحية لتعديله";
                    return RedirectToAction(nameof(Index));
                }

                if (!CanEditResearch(research, user))
                {
                    TempData["ErrorMessage"] = "لا يمكن تعديل هذا البحث في حالته الحالية";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var viewModel = _mapper.Map<CreateResearchViewModel>(research);
                viewModel.IsEditMode = true;
                viewModel.ResearchId = id;
                viewModel.CurrentUserId = user.Id;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading edit research page for ID: {ResearchId}", id);
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحميل صفحة تعديل البحث";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Research/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Researcher,Admin")]
        public async Task<IActionResult> Edit(int id, CreateResearchViewModel model, List<IFormFile> files)
        {
            try
            {
                if (id != model.ResearchId)
                {
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = await GetCurrentUserAsync();
                if (user == null)
                    return RedirectToAction("Login", "Account");

                // Handle file uploads if any
                if (files?.Any() == true)
                {
                    var uploadedFiles = new List<ResearchFileDto>();
                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            var fileResult = await _fileService.UploadFileAsync(file, "research");
                            if (fileResult.Success)
                            {
                                uploadedFiles.Add(new ResearchFileDto
                                {
                                    FileName = fileResult.FileName!,
                                    OriginalFileName = file.FileName,
                                    FilePath = fileResult.FilePath!,
                                    ContentType = file.ContentType,
                                    FileSize = file.Length,
                                    FileType = GetFileType(file.ContentType),
                                    Description = "ملف محدث"
                                });
                            }
                        }
                    }
                    // Add new files to existing ones
                    model.Files = uploadedFiles;
                }

                var updateResearchDto = _mapper.Map<CreateResearchDto>(model);
                
                var command = new UpdateResearchCommand
                {
                    ResearchId = id,
                    Research = updateResearchDto,
                    UserId = user.Id
                };

                await _mediator.Send(command);

                TempData["SuccessMessage"] = "تم تحديث البحث بنجاح";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating research with ID: {ResearchId}", id);
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحديث البحث";
                return View(model);
            }
        }

        // POST: Research/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Researcher,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var query = new GetResearchByIdQuery(id, user.Id);
                var research = await _mediator.Send(query);

                if (research == null)
                {
                    TempData["ErrorMessage"] = "البحث غير موجود";
                    return RedirectToAction(nameof(Index));
                }

                if (!CanDeleteResearch(research, user))
                {
                    TempData["ErrorMessage"] = "لا يمكن حذف هذا البحث";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var command = new DeleteResearchCommand
                {
                    ResearchId = id,
                    UserId = user.Id
                };

                await _mediator.Send(command);

                TempData["SuccessMessage"] = "تم حذف البحث بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting research with ID: {ResearchId}", id);
                TempData["ErrorMessage"] = "حدث خطأ أثناء حذف البحث";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // GET: Research/DownloadFile/5
        public async Task<IActionResult> DownloadFile(int fileId)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var fileResult = await _fileService.GetFileAsync(fileId, user.Id);
                if (!fileResult.Success || fileResult.FileStream == null)
                {
                    TempData["ErrorMessage"] = "الملف غير موجود أو ليس لديك صلاحية للوصول إليه";
                    return RedirectToAction(nameof(Index));
                }

                return File(fileResult.FileStream, fileResult.ContentType!, fileResult.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while downloading file with ID: {FileId}", fileId);
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحميل الملف";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Research/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TrackManager,Admin")]
        public async Task<IActionResult> UpdateStatus(int researchId, ResearchStatus newStatus, string? notes)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var command = new UpdateResearchStatusCommand
                {
                    ResearchId = researchId,
                    NewStatus = newStatus,
                    Notes = notes,
                    UserId = user.Id
                };

                await _mediator.Send(command);

                TempData["SuccessMessage"] = "تم تحديث حالة البحث بنجاح";
                return RedirectToAction(nameof(Details), new { id = researchId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating research status for ID: {ResearchId}", researchId);
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحديث حالة البحث";
                return RedirectToAction(nameof(Details), new { id = researchId });
            }
        }

        // GET: Research/MyResearches
        [Authorize(Roles = "Researcher,Admin")]
        public async Task<IActionResult> MyResearches()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var query = new GetResearchListQuery(user.Id, user.Role);
                var result = await _mediator.Send(query);

                var viewModel = new ResearchListViewModel
                {
                    Researches = result,
                    CurrentUserId = user.Id,
                    CurrentUserRole = user.Role,
                    CanCreateResearch = true,
                    CanManageResearches = false
                };

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading user's researches");
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحميل بحوثك";
                return RedirectToAction(nameof(Index));
            }
        }

        #region Helper Methods

        private List<SelectListItem> GetStatusOptions()
        {
            return Enum.GetValues<ResearchStatus>()
                .Select(s => new SelectListItem
                {
                    Value = ((int)s).ToString(),
                    Text = GetStatusDisplayName(s)
                }).ToList();
        }

        private List<SelectListItem> GetTrackOptions()
        {
            return Enum.GetValues<ResearchTrack>()
                .Select(t => new SelectListItem
                {
                    Value = ((int)t).ToString(),
                    Text = GetTrackDisplayName(t)
                }).ToList();
        }

        private static string GetStatusDisplayName(ResearchStatus status) => status switch
        {
            ResearchStatus.Submitted => "مُقدم",
            ResearchStatus.UnderReview => "قيد المراجعة",
            ResearchStatus.Accepted => "مقبول",
            ResearchStatus.Rejected => "مرفوض",
            ResearchStatus.RequiresRevision => "يتطلب تعديل",
            ResearchStatus.Published => "منشور",
            _ => status.ToString()
        };

        private static string GetTrackDisplayName(ResearchTrack track) => track switch
        {
            ResearchTrack.ComputerScience => "علوم الحاسوب",
            ResearchTrack.InformationSystems => "نظم المعلومات",
            ResearchTrack.SoftwareEngineering => "هندسة البرمجيات",
            ResearchTrack.ArtificialIntelligence => "الذكاء الاصطناعي",
            ResearchTrack.DataScience => "علوم البيانات",
            ResearchTrack.Cybersecurity => "الأمن السيبراني",
            ResearchTrack.NetworkSystems => "أنظمة الشبكات",
            ResearchTrack.HumanComputerInteraction => "التفاعل بين الإنسان والحاسوب",
            _ => track.ToString()
        };

        private static FileType GetFileType(string contentType) => contentType.ToLower() switch
        {
            "application/pdf" => FileType.PDF,
            "application/msword" or "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => FileType.Document,
            "image/jpeg" or "image/png" or "image/gif" => FileType.Image,
            _ => FileType.Other
        };

        private static bool CanCreateResearch(UserRole role) => 
            role == UserRole.Researcher || role == UserRole.Admin;

        private static bool CanManageResearches(UserRole role) => 
            role == UserRole.TrackManager || role == UserRole.Admin;

        private static bool CanEditResearch(ResearchDto research, User user)
        {
            if (user.Role == UserRole.Admin) return true;
            
            if (research.SubmittedById == user.Id)
            {
                return research.Status == ResearchStatus.Submitted || 
                       research.Status == ResearchStatus.RequiresRevision;
            }
            
            return false;
        }

        private static bool CanDeleteResearch(ResearchDto research, User user)
        {
            if (user.Role == UserRole.Admin) return true;
            
            return research.SubmittedById == user.Id && 
                   research.Status == ResearchStatus.Submitted;
        }

        private static bool CanReviewResearch(ResearchDto research, User user)
        {
            if (user.Role != UserRole.Reviewer && user.Role != UserRole.Admin) return false;
            
            return research.Reviews?.Any(r => r.ReviewerId == user.Id && !r.IsCompleted) == true;
        }

        private static bool CanManageStatus(UserRole role) => 
            role == UserRole.TrackManager || role == UserRole.Admin;

        private static bool CanDownloadFiles(ResearchDto research, User user)
        {
            // Authors, reviewers, track managers, and admins can download files
            return research.SubmittedById == user.Id ||
                   research.Authors?.Any(a => a.UserId == user.Id) == true ||
                   research.Reviews?.Any(r => r.ReviewerId == user.Id) == true ||
                   research.AssignedTrackManagerId == user.Id ||
                   user.Role == UserRole.Admin;
        }

        private static bool CanUploadFiles(ResearchDto research, User user)
        {
            // Only authors can upload files, and only in certain statuses
            return research.SubmittedById == user.Id &&
                   (research.Status == ResearchStatus.Submitted || 
                    research.Status == ResearchStatus.RequiresRevision);
        }

        private static bool IsAuthor(ResearchDto research, string userId) => 
            research.SubmittedById == userId || 
            research.Authors?.Any(a => a.UserId == userId) == true;

        private static bool IsReviewer(ResearchDto research, string userId) => 
            research.Reviews?.Any(r => r.ReviewerId == userId) == true;

        private static bool IsTrackManager(ResearchDto research, string userId) => 
            research.AssignedTrackManagerId == userId;

        #endregion
    }

    // Additional Commands that might be needed
    public class UpdateResearchCommand : IRequest<bool>
    {
        public int ResearchId { get; set; }
        public CreateResearchDto Research { get; set; } = new();
        public string UserId { get; set; } = string.Empty;
    }

    public class DeleteResearchCommand : IRequest<bool>
    {
        public int ResearchId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}