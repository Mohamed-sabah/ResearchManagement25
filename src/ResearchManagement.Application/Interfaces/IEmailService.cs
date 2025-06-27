using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ResearchManagement.Domain.Enums;



namespace ResearchManagement.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendReviewAssignmentNotificationAsync(int reviewId);
        Task SendReviewCompletedNotificationAsync(int reviewId);
        Task SendResearchStatusUpdateAsync(int researchId, ResearchStatus oldStatus, ResearchStatus newStatus);
        Task SendReviewerRemovalNotificationAsync(string reviewerId, string researchTitle, string? reason);
        Task SendResearchSubmissionConfirmationAsync(int researchId);
        Task SendDeadlineReminderAsync(int reviewId, int daysRemaining);
        Task SendOverdueNotificationAsync(int reviewId);
        Task SendWelcomeEmailAsync(string userId);
        Task SendPasswordResetEmailAsync(string userId, string resetToken);
    }
}