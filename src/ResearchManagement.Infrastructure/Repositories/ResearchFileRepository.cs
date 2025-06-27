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
    public class ResearchFileRepository : GenericRepository<ResearchFile>, IResearchFileRepository
    {
        public ResearchFileRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ResearchFile?> GetByIdWithAccessCheckAsync(int fileId, string userId)
        {
            var file = await _context.ResearchFiles
                .Include(f => f.Research)
                    .ThenInclude(r => r.SubmittedBy)
                .Include(f => f.Research)
                    .ThenInclude(r => r.Authors)
                .Include(f => f.Research)
                    .ThenInclude(r => r.Reviews)
                .Include(f => f.Research)
                    .ThenInclude(r => r.AssignedTrackManager)
                        .ThenInclude(tm => tm.User)
                .FirstOrDefaultAsync(f => f.Id == fileId && f.IsActive && !f.IsDeleted);

            if (file == null)
                return null;

            // التحقق من صلاحية الوصول
            if (!await CanUserAccessFileAsync(fileId, userId))
                return null;

            return file;
        }

        public async Task<IEnumerable<ResearchFile>> GetByResearchIdAsync(int researchId)
        {
            return await _context.ResearchFiles
                .Where(f => f.ResearchId == researchId && f.IsActive && !f.IsDeleted)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> CanUserAccessFileAsync(int fileId, string userId)
        {
            var file = await _context.ResearchFiles
                .Include(f => f.Research)
                    .ThenInclude(r => r.SubmittedBy)
                .Include(f => f.Research)
                    .ThenInclude(r => r.Authors)
                .Include(f => f.Research)
                    .ThenInclude(r => r.Reviews)
                .Include(f => f.Research)
                    .ThenInclude(r => r.AssignedTrackManager)
                .FirstOrDefaultAsync(f => f.Id == fileId && f.IsActive && !f.IsDeleted);

            if (file == null)
                return false;

            var research = file.Research;

            // المؤلف الرئيسي
            if (research.SubmittedById == userId)
                return true;

            // المؤلفون المشاركون
            if (research.Authors.Any(a => a.UserId == userId && !a.IsDeleted))
                return true;

            // المراجعون المعينون
            if (research.Reviews.Any(r => r.ReviewerId == userId && !r.IsDeleted))
                return true;

            // مدير المسار
            if (research.AssignedTrackManager?.UserId == userId)
                return true;

            // التحقق من دور المستخدم (SystemAdmin)
            var user = await _context.Users.FindAsync(userId);
            if (user?.Role == UserRole.SystemAdmin)
                return true;

            return false;
        }
    }
}