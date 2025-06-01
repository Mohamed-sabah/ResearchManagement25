using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Application.Interfaces
{
    public interface IResearchRepository
    {
        Task<Research?> GetByIdAsync(int id);
        Task<Research?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Research>> GetAllAsync();
        Task<IEnumerable<Research>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Research>> GetByTrackAsync(ResearchTrack track);
        Task<IEnumerable<Research>> GetByStatusAsync(ResearchStatus status);
        Task<IEnumerable<Research>> GetByTrackManagerAsync(int trackManagerId);
        Task<IEnumerable<Research>> GetForReviewerAsync(string reviewerId);
        Task AddAsync(Research research);
        Task UpdateAsync(Research research);
        Task DeleteAsync(int id);
        Task<int> GetCountByStatusAsync(ResearchStatus status);
        Task<int> GetCountByTrackAsync(ResearchTrack track);
        Task<bool> ExistsAsync(int id);
    }

    public interface IReviewRepository
    {
        Task<Review?> GetByIdAsync(int id);
        Task<Review?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Review>> GetByResearchIdAsync(int researchId);
        Task<IEnumerable<Review>> GetByReviewerIdAsync(string reviewerId);
        Task<IEnumerable<Review>> GetPendingReviewsAsync(string reviewerId);
        Task AddAsync(Review review);
        Task UpdateAsync(Review review);
        Task DeleteAsync(int id);
        Task<int> GetCompletedReviewsCountAsync(int researchId);
        Task<decimal> GetAverageScoreAsync(int researchId);
    }

    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetByRoleAsync(UserRole role);
        Task<IEnumerable<User>> GetReviewersByTrackAsync(ResearchTrack track);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(string id);
    }

    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }

    public interface IEmailService
    {
        Task SendResearchSubmissionConfirmationAsync(int researchId);
        Task SendResearchStatusUpdateAsync(int researchId, ResearchStatus oldStatus, ResearchStatus newStatus);
        Task SendReviewAssignmentAsync(int reviewId);
        Task SendReviewCompletedNotificationAsync(int reviewId);
        Task SendDeadlineReminderAsync(string userId, string subject, string message);
        Task SendBulkNotificationAsync(IEnumerable<string> userIds, string subject, string message);
    }

    public interface IFileService
    {
        Task<string> UploadFileAsync(byte[] fileContent, string fileName, string contentType);
        Task<byte[]> DownloadFileAsync(string filePath);
        Task DeleteFileAsync(string filePath);
        Task<bool> FileExistsAsync(string filePath);
        string GetFileUrl(string filePath);
    }
}


