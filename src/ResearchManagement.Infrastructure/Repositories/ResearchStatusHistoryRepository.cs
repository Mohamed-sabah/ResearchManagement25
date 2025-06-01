using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;
using ResearchManagement.Infrastructure.Data;

namespace ResearchManagement.Infrastructure.Repositories
{
    public class ResearchStatusHistoryRepository : IResearchStatusHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public ResearchStatusHistoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ResearchStatusHistory?> GetByIdAsync(int id)
        {
            return await _context.ResearchStatusHistories.FindAsync(id);
        }

        public async Task<IEnumerable<ResearchStatusHistory>> GetByResearchIdAsync(int researchId)
        {
            return await _context.ResearchStatusHistories
                .Include(sh => sh.ChangedBy)
                .Where(sh => sh.ResearchId == researchId)
                .OrderByDescending(sh => sh.ChangedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ResearchStatusHistory>> GetByUserIdAsync(string userId)
        {
            return await _context.ResearchStatusHistories
                .Include(sh => sh.Research)
                .Where(sh => sh.ChangedById == userId)
                .OrderByDescending(sh => sh.ChangedAt)
                .ToListAsync();
        }

        public async Task<ResearchStatusHistory?> GetLatestByResearchIdAsync(int researchId)
        {
            return await _context.ResearchStatusHistories
                .Include(sh => sh.ChangedBy)
                .Where(sh => sh.ResearchId == researchId)
                .OrderByDescending(sh => sh.ChangedAt)
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(ResearchStatusHistory statusHistory)
        {
            await _context.ResearchStatusHistories.AddAsync(statusHistory);
        }

        public async Task UpdateAsync(ResearchStatusHistory statusHistory)
        {
            _context.ResearchStatusHistories.Update(statusHistory);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var statusHistory = await GetByIdAsync(id);
            if (statusHistory != null)
            {
                statusHistory.IsDeleted = true;
                statusHistory.UpdatedAt = DateTime.UtcNow;
            }
        }

        public async Task<int> GetStatusChangeCountAsync(int researchId, ResearchStatus status)
        {
            return await _context.ResearchStatusHistories
                .CountAsync(sh => sh.ResearchId == researchId && sh.ToStatus == status);
        }

        public async Task<IEnumerable<ResearchStatusHistory>> GetHistoryBetweenDatesAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.ResearchStatusHistories
                .Include(sh => sh.Research)
                .Include(sh => sh.ChangedBy)
                .Where(sh => sh.ChangedAt >= startDate && sh.ChangedAt <= endDate)
                .OrderByDescending(sh => sh.ChangedAt)
                .ToListAsync();
        }
    }
}
