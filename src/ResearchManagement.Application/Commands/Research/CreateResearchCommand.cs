using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Application.Commands.Research
{
    public class CreateResearchCommand : IRequest<int>
    {
        public CreateResearchDto Research { get; set; } = new();
        public string UserId { get; set; } = string.Empty;
    }

    //public class CreateResearchCommandHandler : IRequestHandler<CreateResearchCommand, int>
    //{
    //    private readonly IResearchRepository _researchRepository;
    //    private readonly IUnitOfWork _unitOfWork;
    //    private readonly IEmailService _emailService;
    //    private readonly IMapper _mapper;


    //    public CreateResearchCommandHandler(
    //        IResearchRepository researchRepository,
    //        IUnitOfWork unitOfWork,
    //        IEmailService emailService,
    //        IMapper mapper)
    //    {
    //        _researchRepository = researchRepository;
    //        _unitOfWork = unitOfWork;
    //        _emailService = emailService;
    //        _mapper = mapper;

    //    }
    public class CreateResearchCommandHandler : IRequestHandler<CreateResearchCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateResearchCommandHandler> _logger;
        private readonly IEmailService _emailService;

        public CreateResearchCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CreateResearchCommandHandler> logger,
            IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _emailService = emailService;
        }
        public async Task<int> Handle(CreateResearchCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("بدء إنشاء بحث جديد للمستخدم {UserId}", request.UserId);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 1. تحويل DTO إلى Entity
        
                var research = _mapper.Map<Domain.Entities.Research>(request.Research);
                research.SubmittedById = request.UserId;
                research.Status = ResearchStatus.Submitted;
                research.SubmissionDate = DateTime.UtcNow;
                research.CreatedAt = DateTime.UtcNow;
                research.CreatedBy = request.UserId;

                _logger.LogInformation("تم تحويل البيانات بنجاح");

                // 2. إضافة البحث إلى قاعدة البيانات

                //var addedResearch = await _unitOfWork.Research.AddAsync(research);
                var addedResearch = await _unitOfWork.Research.AddAsync(research);
                var saveResult1 = await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("تم حفظ البحث - المعرف: {ResearchId}, النتيجة: {SaveResult}",
                    research.Id, saveResult1);

                if (research.Id == 0)
                {
                    throw new InvalidOperationException("فشل في الحصول على معرف البحث");
                }

                // 3. إضافة المؤلفين
                if (request.Research.Authors?.Any() == true)
                {
                    foreach (var authorDto in request.Research.Authors)
                    {
                        var author = _mapper.Map<ResearchAuthor>(authorDto);
                        author.ResearchId = research.Id;
                        author.CreatedAt = DateTime.UtcNow;
                        author.CreatedBy = request.UserId;

                        research.Authors.Add(author);

                        _logger.LogInformation("تم إضافة مؤلف: {AuthorName}", author.FirstName);
                    }

                    var saveResult2 = await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("تم حفظ المؤلفين - النتيجة: {SaveResult}", saveResult2);
                }





				// 4. persist uploaded files
				if (request.Research.Files?.Any() == true)
				{
					foreach (var fileDto in request.Research.Files)
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
							Description = fileDto.Description,
							Version = 1,
							IsActive = true,
							CreatedAt = DateTime.UtcNow,
							CreatedBy = request.UserId
						};
						research.Files.Add(fileEntity);
					}
					var saveResult3 = await _unitOfWork.SaveChangesAsync(cancellationToken);
					_logger.LogInformation("تم حفظ الملفات – النتيجة: {SaveResult}", saveResult3);
				}



				// 4. إرسال إيميل التأكيد
				try
                {
                    await _emailService.SendResearchSubmissionConfirmationAsync(research.Id);
                    _logger.LogInformation("تم إرسال إيميل التأكيد");
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "فشل في إرسال إيميل التأكيد");
                    // لا نوقف العملية بسبب خطأ الإيميل
                }

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("تم إنشاء البحث بنجاح - المعرف: {ResearchId}", research.Id);
                return research.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل في إنشاء البحث للمستخدم {UserId}", request.UserId);

                await _unitOfWork.RollbackTransactionAsync();
                throw new InvalidOperationException($"فشل في حفظ البحث: {ex.Message}", ex);
            }
        }


        //public async Task<int> Handle(CreateResearchCommand request, CancellationToken cancellationToken)
        //{
        //    await _unitOfWork.BeginTransactionAsync();

        //    try
        //    {
        //        var research = _mapper.Map<Domain.Entities.Research>(request.Research);
        //        research.SubmittedById = request.UserId;
        //        research.Status = Domain.Enums.ResearchStatus.Submitted;
        //        research.SubmissionDate = DateTime.UtcNow;
        //        research.CreatedAt = DateTime.UtcNow;
        //        research.CreatedBy = request.UserId;

        //        // حفظ البحث أولاً للحصول على ID
        //        await _researchRepository.AddAsync(research);
        //        await _unitOfWork.SaveChangesAsync(cancellationToken);

        //        // الآن إضافة المؤلفين
        //        if (request.Research.Authors?.Any() == true)
        //        {
        //            foreach (var authorDto in request.Research.Authors)
        //            {
        //                var author = _mapper.Map<Domain.Entities.ResearchAuthor>(authorDto);
        //                author.ResearchId = research.Id; // الآن له قيمة صحيحة
        //                author.CreatedAt = DateTime.UtcNow;
        //                author.CreatedBy = request.UserId;
        //                research.Authors.Add(author);
        //            }
        //            await _unitOfWork.SaveChangesAsync(cancellationToken);
        //        }

        //        // إرسال الإيميل
        //        try
        //        {
        //            await _emailService.SendResearchSubmissionConfirmationAsync(research.Id);
        //        }
        //        catch (Exception emailEx)
        //        {
        //            // تسجيل خطأ الإيميل دون إيقاف العملية
        //        }

        //        await _unitOfWork.CommitTransactionAsync();

        //        return research.Id;
        //    }
        //    catch (Exception ex)
        //    {
        //        await _unitOfWork.RollbackTransactionAsync();

        //        throw new InvalidOperationException($"فشل في حفظ البحث: {ex.Message}", ex);
        //    }
        //}



    }
}

