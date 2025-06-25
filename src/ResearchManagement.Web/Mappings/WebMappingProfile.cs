using AutoMapper;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Web.Models.ViewModels.Research;

namespace ResearchManagement.Web.Mappings
{
    public class WebMappingProfile : Profile
    {
        public WebMappingProfile()
        {
            // ViewModel to DTO Mappings
            CreateMap<CreateResearchViewModel, CreateResearchDto>()
                .ForMember(dest => dest.Files, opt => opt.Ignore())
                .ForMember(dest => dest.Authors, opt => opt.MapFrom(src => src.Authors));

            CreateMap<ResearchAuthorViewModel, CreateResearchAuthorDto>();

            // DTO to ViewModel Mappings
            CreateMap<ResearchDto, CreateResearchViewModel>()
                .ForMember(dest => dest.IsEditMode, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.ResearchId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Notes, opt => opt.Ignore()) // إضافة هذا الحقل
                .ForMember(dest => dest.Files, opt => opt.Ignore()) // تجاهل الملفات في التحويل
                .ForMember(dest => dest.Authors, opt => opt.MapFrom(src => src.Authors
                    .Select(a => new ResearchAuthorViewModel
                    {
                        FirstName = a.FirstName,
                        LastName = a.LastName,
                        FirstNameEn = a.FirstNameEn,
                        LastNameEn = a.LastNameEn,
                        Email = a.Email,
                        Institution = a.Institution,
                        AcademicDegree = a.AcademicDegree,
                        OrcidId = a.OrcidId,
                        Order = a.Order,
                        IsCorresponding = a.IsCorresponding,
                        UserId = a.UserId
                    }).ToList()));

            CreateMap<ResearchAuthorDto, ResearchAuthorViewModel>();

            // إضافة: Reverse mapping للمؤلفين
            CreateMap<ResearchAuthorViewModel, ResearchAuthorDto>();

            // إضافة: تحويل CreateResearchDto إلى ViewModel (للعرض في صفحة التعديل)
            CreateMap<CreateResearchDto, CreateResearchViewModel>()
                .ForMember(dest => dest.IsEditMode, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.ResearchId, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentUserId, opt => opt.Ignore())
                .ForMember(dest => dest.Notes, opt => opt.Ignore())
                .ForMember(dest => dest.Files, opt => opt.Ignore())
                .ForMember(dest => dest.ResearchTypeOptions, opt => opt.Ignore())
                .ForMember(dest => dest.LanguageOptions, opt => opt.Ignore())
                .ForMember(dest => dest.TrackOptions, opt => opt.Ignore())
                .ForMember(dest => dest.Authors, opt => opt.MapFrom(src => src.Authors
                    .Select(a => new ResearchAuthorViewModel
                    {
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
                    }).ToList()));
        }
    }
}