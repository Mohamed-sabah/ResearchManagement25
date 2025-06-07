using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendResearchSubmissionConfirmationAsync(int researchId);
        Task SendResearchStatusUpdateAsync(int researchId, ResearchStatus oldStatus, ResearchStatus newStatus);
        Task SendReviewAssignmentAsync(int reviewId);
        Task SendReviewCompletedNotificationAsync(int reviewId);
        Task SendDeadlineReminderAsync(string userId, string subject, string message);
        Task SendBulkNotificationAsync(IEnumerable<string> userIds, string subject, string message);
    }
}