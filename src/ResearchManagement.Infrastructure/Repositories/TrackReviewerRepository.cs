using Microsoft.EntityFrameworkCore;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;
using ResearchManagement.Infrastructure.Data;
using ResearchManagement.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TrackReviewerRepository : GenericRepository<TrackReviewer>, ITrackReviewerRepository
{
    public TrackReviewerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TrackReviewer>> GetByTrackAsync(ResearchTrack track)
    {
        return await _context.TrackReviewers
            .Include(tr => tr.Reviewer)
            .Include(tr => tr.TrackManager)
                .ThenInclude(tm => tm.User)
            .Where(tr => tr.Track == track &&
                        tr.IsActive &&
            !tr.IsDeleted)
            .OrderBy(tr => tr.Reviewer.FirstName)
            .ThenBy(tr => tr.Reviewer.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<TrackReviewer>> GetByReviewerIdAsync(string reviewerId)
    {
        return await _context.TrackReviewers
            .Include(tr => tr.TrackManager)
                .ThenInclude(tm => tm.User)
            .Where(tr => tr.ReviewerId == reviewerId &&
                        tr.IsActive &&
                        !tr.IsDeleted)
            .ToListAsync();
    }

    public async Task<TrackReviewer?> GetByTrackAndReviewerAsync(ResearchTrack track, string reviewerId)
    {
        return await _context.TrackReviewers
            .Include(tr => tr.Reviewer)
            .Include(tr => tr.TrackManager)
            .FirstOrDefaultAsync(tr => tr.Track == track &&
            tr.ReviewerId == reviewerId &&
                                      tr.IsActive &&
                                      !tr.IsDeleted);
    }

    public async Task<bool> IsReviewerInTrackAsync(string reviewerId, ResearchTrack track)
    {
        return await _context.TrackReviewers
            .AnyAsync(tr => tr.ReviewerId == reviewerId &&
                           tr.Track == track &&
                           tr.IsActive &&
                           !tr.IsDeleted);
    }
}
