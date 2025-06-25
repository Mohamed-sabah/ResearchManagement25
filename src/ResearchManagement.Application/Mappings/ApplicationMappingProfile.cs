using AutoMapper;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Domain.Entities;

namespace ResearchManagement.Application.Mappings
{
    public class ApplicationMappingProfile : Profile
    {
        public ApplicationMappingProfile()
        {
            // User Mappings
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.FullNameEn, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.FirstNameEn) && !string.IsNullOrEmpty(src.LastNameEn)
                        ? $"{src.FirstNameEn} {src.LastNameEn}" : null));

            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Research Mappings
            CreateMap<Research, ResearchDto>()
                .ForMember(dest => dest.SubmittedByName, opt => opt.MapFrom(src =>
                    $"{src.SubmittedBy.FirstName} {src.SubmittedBy.LastName}"));

            CreateMap<CreateResearchDto, Research>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.SubmissionDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Domain.Enums.ResearchStatus.Submitted))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.SubmittedById, opt => opt.Ignore())
                .ForMember(dest => dest.SubmittedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Files, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.StatusHistory, opt => opt.Ignore())
                .ForMember(dest => dest.Authors, opt => opt.Ignore());

            CreateMap<UpdateResearchDto, Research>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.SubmissionDate, opt => opt.Ignore())
                .ForMember(dest => dest.SubmittedById, opt => opt.Ignore())
                .ForMember(dest => dest.SubmittedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Files, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.StatusHistory, opt => opt.Ignore());

            // تحديث: إضافة Reverse Mapping للتعديل
            CreateMap<Research, CreateResearchDto>()
                .ForMember(dest => dest.Authors, opt => opt.MapFrom(src => src.Authors
                    .Where(a => !a.IsDeleted)
                    .OrderBy(a => a.Order)))
                .ForMember(dest => dest.Files, opt => opt.Ignore());

            // Research Author Mappings
            CreateMap<ResearchAuthor, ResearchAuthorDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.FullNameEn, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.FirstNameEn) && !string.IsNullOrEmpty(src.LastNameEn)
                        ? $"{src.FirstNameEn} {src.LastNameEn}" : null));

            CreateMap<CreateResearchAuthorDto, ResearchAuthor>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ResearchId, opt => opt.Ignore())
                .ForMember(dest => dest.Research, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            // تحديث: إضافة Reverse Mapping للمؤلفين
            CreateMap<ResearchAuthor, CreateResearchAuthorDto>();

            CreateMap<UpdateResearchAuthorDto, ResearchAuthor>()
                .ForMember(dest => dest.ResearchId, opt => opt.Ignore())
                .ForMember(dest => dest.Research, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Review Mappings
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.ReviewerName, opt => opt.MapFrom(src =>
                    $"{src.Reviewer.FirstName} {src.Reviewer.LastName}"))
                .ForMember(dest => dest.ResearchTitle, opt => opt.MapFrom(src => src.Research.Title))
                .ForMember(dest => dest.Score, opt => opt.MapFrom(src => src.OverallScore));

            CreateMap<CreateReviewDto, Review>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewerId, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Deadline, opt => opt.MapFrom(src => DateTime.UtcNow.AddDays(21)))
                .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.RequiresReReview, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Research, opt => opt.Ignore())
                .ForMember(dest => dest.Reviewer, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewFiles, opt => opt.Ignore());

            CreateMap<UpdateReviewDto, Review>()
                .ForMember(dest => dest.ReviewerId, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Deadline, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Research, opt => opt.Ignore())
                .ForMember(dest => dest.Reviewer, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewFiles, opt => opt.Ignore());

            // Research File Mappings
            CreateMap<ResearchFile, ResearchFileDto>();

            CreateMap<UploadFileDto, ResearchFile>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(d => d.ResearchId, o => o.Ignore())
                .ForMember(dest => dest.FileName, opt => opt.Ignore())
                .ForMember(dest => dest.OriginalFileName, opt => opt.MapFrom(src => src.FileName))
                .ForMember(dest => dest.FilePath, opt => opt.Ignore())
                .ForMember(dest => dest.FileSize, opt => opt.MapFrom(src => src.FileContent.Length))
                .ForMember(dest => dest.Version, opt => opt.MapFrom(src => 1))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Research, opt => opt.Ignore())
                .ForMember(dest => dest.Review, opt => opt.Ignore());
        }
    }
}