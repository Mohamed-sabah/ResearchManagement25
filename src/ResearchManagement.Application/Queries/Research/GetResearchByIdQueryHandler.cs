using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Application.Queries.Research
{
    public class GetResearchByIdQueryHandler : IRequestHandler<GetResearchByIdQuery, ResearchDto?>
    {
        private readonly IResearchRepository _researchRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetResearchByIdQueryHandler> _logger;

        public GetResearchByIdQueryHandler(
            IResearchRepository researchRepository,
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<GetResearchByIdQueryHandler> logger)
        {
            _researchRepository = researchRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResearchDto?> Handle(GetResearchByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("جاري البحث عن البحث بالمعرف {ResearchId}", request.Id);

            var research = await _researchRepository.GetByIdWithDetailsAsync(request.Id);

            if (research == null)
            {
                _logger.LogWarning("لم يتم العثور على البحث بالمعرف {ResearchId}", request.Id);
                return null;
            }

            // التحقق من صلاحية الوصول إذا تم تمرير معرف المستخدم
            if (!string.IsNullOrEmpty(request.UserId))
            {
                var hasAccess = await CheckUserAccessAsync(research, request.UserId);
                if (!hasAccess)
                {
                    _logger.LogWarning("المستخدم {UserId} لا يملك صلاحية الوصول للبحث {ResearchId}", 
                        request.UserId, request.Id);
                    return null;
                }
            }

            var researchDto = _mapper.Map<ResearchDto>(research);
            
            _logger.LogInformation("تم العثور على البحث {ResearchId} بنجاح", request.Id);
            
            return researchDto;
        }

        private async Task<bool> CheckUserAccessAsync(Domain.Entities.Research research, string userId)
        {
            // المؤلف الرئيسي يمكنه الوصول
            if (research.SubmittedById == userId)
                return true;

            // المؤلفون المشاركون يمكنهم الوصول
            if (research.Authors?.Any(a => a.UserId == userId) == true)
                return true;

            // مدير المسار يمكنه الوصول
            if (int.TryParse(userId, out int userIdInt) && research.AssignedTrackManagerId == userIdInt)
                return true;

            // المراجعون المعينون يمكنهم الوصول
            if (research.Reviews?.Any(r => r.ReviewerId == userId) == true)
                return true;

            // الأدمن يمكنه الوصول لكل شيء
            var user = await _userRepository.GetByIdAsync(userId);
            if (user?.Role == UserRole.SystemAdmin)
                return true;

            return false;
        }
    }
}