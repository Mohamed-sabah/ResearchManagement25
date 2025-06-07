using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Application.Interfaces
{
    public interface IResearchStatusHistoryRepository
    {
        Task<ResearchStatusHistory?> GetByIdAsync(int id);
        Task<IEnumerable<ResearchStatusHistory>> GetByResearchIdAsync(int researchId);
        Task<IEnumerable<ResearchStatusHistory>> GetByUserIdAsync(string userId);
        Task<ResearchStatusHistory?> GetLatestByResearchIdAsync(int researchId);
        Task AddAsync(ResearchStatusHistory statusHistory);
        Task UpdateAsync(ResearchStatusHistory statusHistory);
        Task DeleteAsync(int id);
        Task<int> GetStatusChangeCountAsync(int researchId, ResearchStatus status);
        Task<IEnumerable<ResearchStatusHistory>> GetHistoryBetweenDatesAsync(DateTime startDate, DateTime endDate);
    }
}
