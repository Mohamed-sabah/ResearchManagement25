using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchManagement.Application.Interfaces
{
    public interface IResearchFileRepository : IGenericRepository<ResearchFile>
    {
        Task<ResearchFile?> GetByIdWithAccessCheckAsync(int fileId, string userId);
        Task<IEnumerable<ResearchFile>> GetByResearchIdAsync(int researchId);
        Task<bool> CanUserAccessFileAsync(int fileId, string userId);
    }

    public interface IResearchRepository : IGenericRepository<Research>
    {
        Task<Research?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Research>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Research>> GetByTrackAsync(ResearchTrack track);
        Task<PagedResult<Research>> GetPagedByTrackAsync(
            ResearchTrack track,
            string? searchTerm = null,
            ResearchStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int page = 1,
            int pageSize = 10);
        Task<IEnumerable<Research>> GetByTrackAndDateRangeAsync(ResearchTrack track, DateTime fromDate, DateTime toDate);
        Task<bool> CanUserAccessResearchAsync(int researchId, string userId);
    }
}