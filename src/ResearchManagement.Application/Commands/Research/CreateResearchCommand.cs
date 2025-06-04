using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Interfaces;

namespace ResearchManagement.Application.Commands.Research
{
    public class CreateResearchCommand : IRequest<int>
    {
        public CreateResearchDto Research { get; set; } = new();
        public string UserId { get; set; } = string.Empty;
    }

    public class CreateResearchCommandHandler : IRequestHandler<CreateResearchCommand, int>
    {
        private readonly IResearchRepository _researchRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public CreateResearchCommandHandler(
            IResearchRepository researchRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IMapper mapper)
        {
            _researchRepository = researchRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<int> Handle(CreateResearchCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // تحويل DTO إلى Entity باستخدام AutoMapper
                var research = _mapper.Map<Domain.Entities.Research>(request.Research);

                // تعيين معرف المستخدم والتواريخ
                research.SubmittedById = request.UserId;
                research.Status = Domain.Enums.ResearchStatus.Submitted;
                research.SubmissionDate = DateTime.UtcNow;
                research.CreatedAt = DateTime.UtcNow;
                research.CreatedBy = request.UserId;

                // إضافة البحث إلى قاعدة البيانات
                await _researchRepository.AddAsync(research);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // تحديث معرف البحث في المؤلفين
                if (request.Research.Authors?.Any() == true)
                {
                    foreach (var authorDto in request.Research.Authors)
                    {
                        var author = _mapper.Map<Domain.Entities.ResearchAuthor>(authorDto);
                        author.ResearchId = research.Id;
                        author.CreatedAt = DateTime.UtcNow;
                        author.CreatedBy = request.UserId;

                        research.Authors.Add(author);
                    }

                    // حفظ المؤلفين
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                // إرسال بريد إلكتروني للتأكيد
                try
                {
                    await _emailService.SendResearchSubmissionConfirmationAsync(research.Id);
                }
                catch (Exception emailEx)
                {
                    // تسجيل خطأ الإيميل لكن عدم إيقاف العملية
                    // يمكن استخدام logger هنا
                }

                return research.Id;
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ
                throw new InvalidOperationException($"فشل في حفظ البحث: {ex.Message}", ex);
            }
        }
    }
}
