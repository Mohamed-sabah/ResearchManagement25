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
    public class ResearchRepository : IResearchRepository
    {
        private readonly ApplicationDbContext _context;

        public ResearchRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Research?> GetByIdAsync(int id)
        {
            return await _context.Researches.FindAsync(id);
        }

        public async Task<Research?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Researches
                .Include(r => r.SubmittedBy)
                .Include(r => r.Authors)
                .Include(r => r.Files)
                .Include(r => r.Reviews)
                    .ThenInclude(rv => rv.Reviewer)
                .Include(r => r.StatusHistory)
                    .ThenInclude(sh => sh.ChangedBy)
                .Include(r => r.AssignedTrackManager)
                    .ThenInclude(tm => tm.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Research>> GetAllAsync()
        {
            return await _context.Researches
                .Include(r => r.SubmittedBy)
                .Include(r => r.Authors)
                .OrderByDescending(r => r.SubmissionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Research>> GetByUserIdAsync(string userId)
        {
            return await _context.Researches
                .Include(r => r.Authors)
                .Include(r => r.Files)
                .Include(r => r.Reviews)
                .Where(r => r.SubmittedById == userId)
                .OrderByDescending(r => r.SubmissionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Research>> GetByTrackAsync(ResearchTrack track)
        {
            return await _context.Researches
                .Include(r => r.SubmittedBy)
                .Include(r => r.Authors)
                .Where(r => r.Track == track)
                .OrderByDescending(r => r.SubmissionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Research>> GetByStatusAsync(ResearchStatus status)
        {
            return await _context.Researches
                .Include(r => r.SubmittedBy)
                .Include(r => r.Authors)
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.SubmissionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Research>> GetByTrackManagerAsync(int trackManagerId)
        {
            return await _context.Researches
                .Include(r => r.SubmittedBy)
                .Include(r => r.Authors)
                .Include(r => r.Reviews)
                .Where(r => r.AssignedTrackManagerId == trackManagerId)
                .OrderByDescending(r => r.SubmissionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Research>> GetForReviewerAsync(string reviewerId)
        {
            return await _context.Researches
                .Include(r => r.Authors)
                .Include(r => r.Files)
                .Where(r => r.Reviews.Any(rv => rv.ReviewerId == reviewerId))
                .OrderByDescending(r => r.SubmissionDate)
                .ToListAsync();
        }

        public async Task AddAsync(Research research)
        {
            await _context.Researches.AddAsync(research);
        }

        public async Task UpdateAsync(Research research)
        {
            _context.Researches.Update(research);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var research = await GetByIdAsync(id);
            if (research != null)
            {
                research.IsDeleted = true;
                research.UpdatedAt = DateTime.UtcNow;
            }
        }

        public async Task<int> GetCountByStatusAsync(ResearchStatus status)
        {
            return await _context.Researches
                .CountAsync(r => r.Status == status);
        }

        public async Task<int> GetCountByTrackAsync(ResearchTrack track)
        {
            return await _context.Researches
                .CountAsync(r => r.Track == track);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Researches
                .AnyAsync(r => r.Id == id);
        }
    }
}
