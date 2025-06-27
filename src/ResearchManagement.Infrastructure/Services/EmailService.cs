using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Enums;
using ResearchManagement.Infrastructure.Data;
using ResearchManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly IReviewRepository _reviewRepository;
        private readonly IResearchRepository _researchRepository;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger,
            IReviewRepository reviewRepository,
            IResearchRepository researchRepository)
        {
            _configuration = configuration;
            _logger = logger;
            _reviewRepository = reviewRepository;
            _researchRepository = researchRepository;
        }

        public async Task SendReviewAssignmentNotificationAsync(int reviewId)
        {
            try
            {
                var review = await _reviewRepository.GetByIdWithDetailsAsync(reviewId);
                if (review == null)
                    return;

                var subject = $"تكليف مراجعة بحث جديد: {review.Research.Title}";
                var body = $@"
                    <div dir='rtl' style='font-family: Arial, sans-serif;'>
                        <h2>تكليف مراجعة بحث جديد</h2>
                        <p>عزيزي الدكتور {review.Reviewer.FirstName} {review.Reviewer.LastName},</p>
                        <p>تم تكليفكم بمراجعة البحث التالي:</p>
                        
                        <div style='background-color: #f8f9fa; padding: 15px; border-right: 4px solid #007bff; margin: 20px 0;'>
                            <h3>{review.Research.Title}</h3>
                            {(!string.IsNullOrEmpty(review.Research.TitleEn) ? $"<p><strong>العنوان بالإنجليزية:</strong> {review.Research.TitleEn}</p>" : "")}
                            <p><strong>المسار:</strong> {GetTrackDisplayName(review.Research.Track)}</p>
                            <p><strong>تاريخ التكليف:</strong> {review.AssignedDate:dd/MM/yyyy}</p>
                            <p><strong>الموعد النهائي:</strong> {review.Deadline:dd/MM/yyyy}</p>
                        </div>

                        <p>يرجى الدخول إلى النظام لبدء عملية المراجعة.</p>
                        
                        <p>مع أطيب التحيات،<br/>
                        فريق إدارة البحوث</p>
                    </div>";

                await SendEmailAsync(review.Reviewer.Email, subject, body);
                _logger.LogInformation("Review assignment notification sent to {Email} for review {ReviewId}",
                    review.Reviewer.Email, reviewId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send review assignment notification for review {ReviewId}", reviewId);
                throw;
            }
        }

        public async Task SendReviewCompletedNotificationAsync(int reviewId)
        {
            try
            {
                var review = await _reviewRepository.GetByIdWithDetailsAsync(reviewId);
                if (review == null || !review.IsCompleted)
                    return;

                // إشعار للباحث
                var researcherSubject = $"تم إكمال مراجعة بحثكم: {review.Research.Title}";
                var researcherBody = $@"
                    <div dir='rtl' style='font-family: Arial, sans-serif;'>
                        <h2>تم إكمال مراجعة بحثكم</h2>
                        <p>عزيزي الباحث {review.Research.SubmittedBy.FirstName} {review.Research.SubmittedBy.LastName},</p>
                        <p>نود إعلامكم بأنه تم إكمال إحدى مراجعات بحثكم:</p>
                        
                        <div style='background-color: #f8f9fa; padding: 15px; border-right: 4px solid #28a745; margin: 20px 0;'>
                            <h3>{review.Research.Title}</h3>
                            <p><strong>النقاط:</strong> {review.OverallScore:F1}/10</p>
                            <p><strong>القرار:</strong> {GetDecisionDisplayName(review.Decision)}</p>
                            <p><strong>تاريخ الإكمال:</strong> {review.CompletedDate:dd/MM/yyyy}</p>
                        </div>

                        <p>يرجى الدخول إلى النظام لمراجعة التفاصيل الكاملة.</p>
                        
                        <p>مع أطيب التحيات،<br/>
                        فريق إدارة البحوث</p>
                    </div>";

                await SendEmailAsync(review.Research.SubmittedBy.Email, researcherSubject, researcherBody);

                // إشعار لمدير المسار
                if (review.Research.AssignedTrackManager != null)
                {
                    var managerSubject = $"تم إكمال مراجعة في مسارك: {review.Research.Title}";
                    var managerBody = $@"
                        <div dir='rtl' style='font-family: Arial, sans-serif;'>
                            <h2>تم إكمال مراجعة في مسارك</h2>
                            <p>عزيزي مدير المسار,</p>
                            <p>تم إكمال مراجعة للبحث التالي في مسارك:</p>
                            
                            <div style='background-color: #f8f9fa; padding: 15px; border-right: 4px solid #17a2b8; margin: 20px 0;'>
                                <h3>{review.Research.Title}</h3>
                                <p><strong>المراجع:</strong> {review.Reviewer.FirstName} {review.Reviewer.LastName}</p>
                                <p><strong>النقاط:</strong> {review.OverallScore:F1}/10</p>
                                <p><strong>القرار:</strong> {GetDecisionDisplayName(review.Decision)}</p>
                                {(!string.IsNullOrEmpty(review.CommentsToTrackManager) ? $"<p><strong>تعليقات خاصة:</strong> {review.CommentsToTrackManager}</p>" : "")}
                            </div>

                            <p>يرجى الدخول إلى النظام لمراجعة التفاصيل الكاملة.</p>
                            
                            <p>مع أطيب التحيات،<br/>
                            فريق إدارة البحوث</p>
                        </div>";

                    await SendEmailAsync(review.Research.AssignedTrackManager.User.Email, managerSubject, managerBody);
                }

                _logger.LogInformation("Review completion notifications sent for review {ReviewId}", reviewId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send review completion notification for review {ReviewId}", reviewId);
                throw;
            }
        }

        public async Task SendResearchStatusUpdateAsync(int researchId, ResearchStatus oldStatus, ResearchStatus newStatus)
        {
            try
            {
                var research = await _researchRepository.GetByIdWithDetailsAsync(researchId);
                if (research == null)
                    return;

                var subject = $"تحديث حالة البحث: {research.Title}";
                var body = $@"
                    <div dir='rtl' style='font-family: Arial, sans-serif;'>
                        <h2>تحديث حالة البحث</h2>
                        <p>عزيزي الباحث {research.SubmittedBy.FirstName} {research.SubmittedBy.LastName},</p>
                        <p>تم تحديث حالة بحثكم:</p>
                        
                        <div style='background-color: #f8f9fa; padding: 15px; border-right: 4px solid #ffc107; margin: 20px 0;'>
                            <h3>{research.Title}</h3>
                            <p><strong>الحالة السابقة:</strong> {GetStatusDisplayName(oldStatus)}</p>
                            <p><strong>الحالة الجديدة:</strong> {GetStatusDisplayName(newStatus)}</p>
                            <p><strong>تاريخ التحديث:</strong> {DateTime.UtcNow:dd/MM/yyyy HH:mm}</p>
                        </div>

                        {GetStatusUpdateMessage(newStatus)}
                        
                        <p>يرجى الدخول إلى النظام لمراجعة التفاصيل الكاملة.</p>
                        
                        <p>مع أطيب التحيات،<br/>
                        فريق إدارة البحوث</p>
                    </div>";

                await SendEmailAsync(research.SubmittedBy.Email, subject, body);

                _logger.LogInformation("Research status update notification sent for research {ResearchId} to {Email}",
                    researchId, research.SubmittedBy.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send research status update notification for research {ResearchId}", researchId);
                throw;
            }
        }

        public async Task SendReviewerRemovalNotificationAsync(string reviewerId, string researchTitle, string? reason)
        {
            try
            {
                var reviewer = await _context.Users.FindAsync(reviewerId);
                if (reviewer == null)
                    return;

                var subject = $"إلغاء تكليف مراجعة: {researchTitle}";
                var body = $@"
                    <div dir='rtl' style='font-family: Arial, sans-serif;'>
                        <h2>إلغاء تكليف المراجعة</h2>
                        <p>عزيزي الدكتور {reviewer.FirstName} {reviewer.LastName},</p>
                        <p>نود إعلامكم بأنه تم إلغاء تكليفكم بمراجعة البحث التالي:</p>
                        
                        <div style='background-color: #f8f9fa; padding: 15px; border-right: 4px solid #dc3545; margin: 20px 0;'>
                            <h3>{researchTitle}</h3>
                            {(!string.IsNullOrEmpty(reason) ? $"<p><strong>السبب:</strong> {reason}</p>" : "")}
                            <p><strong>تاريخ الإلغاء:</strong> {DateTime.UtcNow:dd/MM/yyyy}</p>
                        </div>

                        <p>نعتذر عن أي إزعاج قد يسببه هذا التغيير.</p>
                        
                        <p>مع أطيب التحيات،<br/>
                        فريق إدارة البحوث</p>
                    </div>";

                await SendEmailAsync(reviewer.Email, subject, body);

                _logger.LogInformation("Reviewer removal notification sent to {Email} for research {ResearchTitle}",
                    reviewer.Email, researchTitle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reviewer removal notification to {ReviewerId}", reviewerId);
                throw;
            }
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // تنفيذ إرسال البريد الإلكتروني الفعلي
            // يمكن استخدام SendGrid, SMTP, أو أي خدمة بريد إلكتروني أخرى

            _logger.LogInformation("Email sent to {Email} with subject: {Subject}", toEmail, subject);

            // مثال على تنفيذ SMTP (يحتاج إلى تكوين إضافي)
            /*
            var smtpClient = new SmtpClient(_configuration["Email:SmtpServer"])
            {
                Port = int.Parse(_configuration["Email:Port"]),
                Credentials = new NetworkCredential(_configuration["Email:Username"], _configuration["Email:Password"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["Email:FromAddress"], _configuration["Email:FromName"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            
            mailMessage.To.Add(toEmail);
            await smtpClient.SendMailAsync(mailMessage);
            */
        }

        private static string GetTrackDisplayName(ResearchTrack track) => track switch
        {
            ResearchTrack.InformationTechnology => "تقنية المعلومات",
            ResearchTrack.InformationSecurity => "أمن المعلومات",
            ResearchTrack.ArtificialIntelligence => "الذكاء الاصطناعي",
            ResearchTrack.DataScience => "علوم البيانات",
            ResearchTrack.SoftwareEngineering => "هندسة البرمجيات",
            ResearchTrack.NetworkingAndCommunications => "الشبكات والاتصالات",
            _ => track.ToString()
        };

        private static string GetDecisionDisplayName(ReviewDecision decision) => decision switch
        {
            ReviewDecision.AcceptAsIs => "قبول فوري",
            ReviewDecision.AcceptWithMinorRevisions => "قبول مع تعديلات طفيفة",
            ReviewDecision.MajorRevisionsRequired => "تعديلات جوهرية مطلوبة",
            ReviewDecision.Reject => "رفض",
            ReviewDecision.NotSuitableForConference => "غير مناسب للمؤتمر",
            ReviewDecision.NotReviewed => "لم يتم المراجعة بعد",
            _ => "غير محدد"
        };

        private static string GetStatusDisplayName(ResearchStatus status) => status switch
        {
            ResearchStatus.Submitted => "مُقدم",
            ResearchStatus.AssignedForReview => "معين للمراجعة",
            ResearchStatus.UnderReview => "قيد المراجعة",
            ResearchStatus.UnderEvaluation => "قيد التقييم",
            ResearchStatus.Accepted => "مقبول",
            ResearchStatus.Rejected => "مرفوض",
            ResearchStatus.RequiresMinorRevisions => "يتطلب تعديلات طفيفة",
            ResearchStatus.RequiresMajorRevisions => "يتطلب تعديلات كبيرة",
            ResearchStatus.RevisionsSubmitted => "تم تقديم التعديلات",
            _ => status.ToString()
        };

        private static string GetStatusUpdateMessage(ResearchStatus newStatus) => newStatus switch
        {
            ResearchStatus.Accepted => "<p style='color: #28a745; font-weight: bold;'>تهانينا! تم قبول بحثكم.</p>",
            ResearchStatus.Rejected => "<p style='color: #dc3545; font-weight: bold;'>نأسف لإبلاغكم بأنه تم رفض البحث.</p>",
            ResearchStatus.RequiresMinorRevisions => "<p style='color: #ffc107; font-weight: bold;'>يرجى إجراء التعديلات الطفيفة المطلوبة وإعادة تقديم البحث.</p>",
            ResearchStatus.RequiresMajorRevisions => "<p style='color: #fd7e14; font-weight: bold;'>يرجى إجراء التعديلات الجوهرية المطلوبة وإعادة تقديم البحث.</p>",
            ResearchStatus.UnderReview => "<p>بحثكم الآن قيد المراجعة من قبل المحكمين.</p>",
            ResearchStatus.UnderEvaluation => "<p>بحثكم الآن قيد التقييم النهائي.</p>",
            _ => ""
        };
    }
}


//namespace ResearchManagement.Infrastructure.Services
//{
//public class EmailService : IEmailService
//{
//    private readonly EmailSettings _emailSettings;
//    private readonly ApplicationDbContext _context;

//    public EmailService(IOptions<EmailSettings> emailSettings, ApplicationDbContext context)
//    {
//        _emailSettings = emailSettings.Value;
//        _context = context;
//    }

//    public async Task SendResearchSubmissionConfirmationAsync(int researchId)
//    {
//        var research = await _context.Researches
//            .Include(r => r.SubmittedBy)
//            .FirstOrDefaultAsync(r => r.Id == researchId);

//        if (research == null) return;

//        var subject = "تأكيد استلام البحث العلمي";
//        var body = $@"
//            <div dir='rtl' style='font-family: Arial, sans-serif;'>
//                <h2>تأكيد استلام البحث العلمي</h2>
//                <p>عزيزي/عزيزتي {research.SubmittedBy.FirstName} {research.SubmittedBy.LastName}،</p>
//                <p>نؤكد لكم استلام بحثكم العلمي بعنوان: <strong>{research.Title}</strong></p>
//                <p>رقم البحث: <strong>{research.Id}</strong></p>
//                <p>تاريخ التقديم: <strong>{research.SubmissionDate:yyyy/MM/dd}</strong></p>
//                <p>التخصص: <strong>{GetTrackDisplayName(research.Track)}</strong></p>
//                <p>الحالة الحالية: <strong>{GetStatusDisplayName(research.Status)}</strong></p>
//                <br>
//                <p>سيتم مراجعة بحثكم وإشعاركم بأي تطورات.</p>
//                <p>مع تحياتنا،<br>فريق إدارة المؤتمر</p>
//            </div>";

//        await SendEmailAsync(research.SubmittedBy.Email, subject, body, NotificationType.ResearchSubmissionConfirmation, researchId, research.SubmittedById);
//    }

//    public async Task SendResearchStatusUpdateAsync(int researchId, ResearchStatus oldStatus, ResearchStatus newStatus)
//    {
//        var research = await _context.Researches
//            .Include(r => r.SubmittedBy)
//            .FirstOrDefaultAsync(r => r.Id == researchId);

//        if (research == null) return;

//        var subject = "تحديث حالة البحث العلمي";
//        var body = $@"
//            <div dir='rtl' style='font-family: Arial, sans-serif;'>
//                <h2>تحديث حالة البحث العلمي</h2>
//                <p>عزيزي/عزيزتي {research.SubmittedBy.FirstName} {research.SubmittedBy.LastName}،</p>
//                <p>نود إعلامكم بتحديث حالة بحثكم العلمي:</p>
//                <p><strong>عنوان البحث:</strong> {research.Title}</p>
//                <p><strong>رقم البحث:</strong> {research.Id}</p>
//                <p><strong>الحالة السابقة:</strong> {GetStatusDisplayName(oldStatus)}</p>
//                <p><strong>الحالة الجديدة:</strong> <span style='color: #007bff;'>{GetStatusDisplayName(newStatus)}</span></p>
//                <br>
//                {GetStatusMessage(newStatus)}
//                <p>مع تحياتنا،<br>فريق إدارة المؤتمر</p>
//            </div>";

//        await SendEmailAsync(research.SubmittedBy.Email, subject, body, NotificationType.ResearchStatusUpdate, researchId, research.SubmittedById);
//    }

//    public async Task SendReviewAssignmentAsync(int reviewId)
//    {
//        var review = await _context.Reviews
//            .Include(r => r.Research)
//            .Include(r => r.Reviewer)
//            .FirstOrDefaultAsync(r => r.Id == reviewId);

//        if (review == null) return;

//        var subject = "تكليف مراجعة بحث علمي";
//        var body = $@"
//            <div dir='rtl' style='font-family: Arial, sans-serif;'>
//                <h2>تكليف مراجعة بحث علمي</h2>
//                <p>عزيزي الدكتور/الدكتورة {review.Reviewer.FirstName} {review.Reviewer.LastName}،</p>
//                <p>نرجو منكم مراجعة البحث التالي:</p>
//                <p><strong>عنوان البحث:</strong> {review.Research.Title}</p>
//                <p><strong>التخصص:</strong> {GetTrackDisplayName(review.Research.Track)}</p>
//                <p><strong>تاريخ التكليف:</strong> {review.AssignedDate:yyyy/MM/dd}</p>
//                <p><strong>الموعد النهائي:</strong> <span style='color: #dc3545;'>{review.Deadline:yyyy/MM/dd}</span></p>
//                <br>
//                <p>يرجى تسجيل الدخول للنظام لتحميل البحث وإجراء المراجعة.</p>
//                <p>مع تحياتنا،<br>فريق إدارة المؤتمر</p>
//            </div>";

//        await SendEmailAsync(review.Reviewer.Email, subject, body, NotificationType.ReviewAssignment, review.ResearchId, review.ReviewerId);
//    }

//    public async Task SendReviewCompletedNotificationAsync(int reviewId)
//    {
//        var review = await _context.Reviews
//            .Include(r => r.Research)
//                .ThenInclude(res => res.SubmittedBy)
//            .Include(r => r.Reviewer)
//            .FirstOrDefaultAsync(r => r.Id == reviewId);

//        if (review == null) return;

//        // إشعار للباحث
//        var researcherSubject = "وصول تعليقات المراجعة";
//        var researcherBody = $@"
//            <div dir='rtl' style='font-family: Arial, sans-serif;'>
//                <h2>وصول تعليقات المراجعة</h2>
//                <p>عزيزي/عزيزتي {review.Research.SubmittedBy.FirstName} {review.Research.SubmittedBy.LastName}،</p>
//                <p>تم الانتهاء من مراجعة بحثكم وإرسال التعليقات:</p>
//                <p><strong>عنوان البحث:</strong> {review.Research.Title}</p>
//                <p><strong>قرار المراجعة:</strong> {GetDecisionDisplayName(review.Decision)}</p>
//                <p><strong>النتيجة الإجمالية:</strong> {review.OverallScore:F2}/10</p>
//                <br>
//                <p>يرجى تسجيل الدخول للنظام لعرض التفاصيل الكاملة.</p>
//                <p>مع تحياتنا،<br>فريق إدارة المؤتمر</p>
//            </div>";

//        await SendEmailAsync(review.Research.SubmittedBy.Email, researcherSubject, researcherBody,
//            NotificationType.ReviewCommentsReceived, review.ResearchId, review.Research.SubmittedById);

//        // إشعار لمدير التراك
//        var trackManager = await _context.TrackManagers
//            .Include(tm => tm.User)
//            .FirstOrDefaultAsync(tm => tm.Track == review.Research.Track && tm.IsActive);

//        if (trackManager != null)
//        {
//            var managerSubject = "اكتمال مراجعة بحث";
//            var managerBody = $@"
//                <div dir='rtl' style='font-family: Arial, sans-serif;'>
//                    <h2>اكتمال مراجعة بحث</h2>
//                    <p>تم الانتهاء من مراجعة البحث التالي:</p>
//                    <p><strong>عنوان البحث:</strong> {review.Research.Title}</p>
//                    <p><strong>المراجع:</strong> {review.Reviewer.FirstName} {review.Reviewer.LastName}</p>
//                    <p><strong>قرار المراجعة:</strong> {GetDecisionDisplayName(review.Decision)}</p>
//                    <p><strong>النتيجة الإجمالية:</strong> {review.OverallScore:F2}/10</p>
//                    <br>
//                    <p>يرجى مراجعة النتائج واتخاذ الإجراء المناسب.</p>
//                </div>";

//            await SendEmailAsync(trackManager.User.Email, managerSubject, managerBody,
//                NotificationType.ReviewCommentsReceived, review.ResearchId, trackManager.UserId);
//        }
//    }

//    public async Task SendDeadlineReminderAsync(string userId, string subject, string message)
//    {
//        var user = await _context.Users.FindAsync(userId);
//        if (user == null) return;

//        var body = $@"
//            <div dir='rtl' style='font-family: Arial, sans-serif;'>
//                <h2>تذكير بموعد نهائي</h2>
//                <p>عزيزي/عزيزتي {user.FirstName} {user.LastName}،</p>
//                <p>{message}</p>
//                <br>
//                <p>مع تحياتنا،<br>فريق إدارة المؤتمر</p>
//            </div>";

//        await SendEmailAsync(user.Email, subject, body, NotificationType.DeadlineReminder, null, userId);
//    }

//    public async Task SendBulkNotificationAsync(IEnumerable<string> userIds, string subject, string message)
//    {
//        var users = await _context.Users
//            .Where(u => userIds.Contains(u.Id))
//            .ToListAsync();

//        foreach (var user in users)
//        {
//            var body = $@"
//                <div dir='rtl' style='font-family: Arial, sans-serif;'>
//                    <h2>{subject}</h2>
//                    <p>عزيزي/عزيزتي {user.FirstName} {user.LastName}،</p>
//                    <p>{message}</p>
//                    <br>
//                    <p>مع تحياتنا،<br>فريق إدارة المؤتمر</p>
//                </div>";

//            await SendEmailAsync(user.Email, subject, body, NotificationType.GeneralNotification, null, user.Id);
//        }
//    }

//    private async Task SendEmailAsync(string toEmail, string subject, string body, NotificationType type, int? researchId = null, string? userId = null)
//    {
//        var notification = new EmailNotification
//        {
//            ToEmail = toEmail,
//            Subject = subject,
//            Body = body,
//            Type = type,
//            Status = NotificationStatus.Pending,
//            ResearchId = researchId,
//            UserId = userId
//        };

//        _context.EmailNotifications.Add(notification);
//        await _context.SaveChangesAsync();

//        try
//        {
//            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
//            client.EnableSsl = _emailSettings.EnableSsl;
//            client.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);

//            var mailMessage = new MailMessage
//            {
//                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
//                Subject = subject,
//                Body = body,
//                IsBodyHtml = true
//            };

//            mailMessage.To.Add(toEmail);

//            await client.SendMailAsync(mailMessage);

//            notification.Status = NotificationStatus.Sent;
//            notification.SentAt = DateTime.UtcNow;
//        }
//        catch (Exception ex)
//        {
//            notification.Status = NotificationStatus.Failed;
//            notification.ErrorMessage = ex.Message;
//            notification.RetryCount++;
//        }

//        await _context.SaveChangesAsync();
//    }

//    private string GetStatusDisplayName(ResearchStatus status)
//    {
//        return status switch
//        {
//            ResearchStatus.Submitted => "مقدم",
//            ResearchStatus.UnderInitialReview => "قيد المراجعة الأولية",
//            ResearchStatus.AssignedForReview => "موزع للتقييم",
//            ResearchStatus.UnderReview => "قيد التقييم",
//            ResearchStatus.UnderEvaluation => "تحت المراجعة",
//            ResearchStatus.RequiresMinorRevisions => "يتطلب تعديلات طفيفة",
//            ResearchStatus.RequiresMajorRevisions => "يتطلب تعديلات جوهرية",
//            ResearchStatus.RevisionsSubmitted => "تعديلات مقدمة",
//            ResearchStatus.RevisionsUnderReview => "مراجعة التعديلات",
//            ResearchStatus.Accepted => "مقبول",
//            ResearchStatus.Rejected => "مرفوض",
//            ResearchStatus.Withdrawn => "منسحب",
//            _ => status.ToString()
//        };
//    }

//    private string GetTrackDisplayName(ResearchTrack track)
//    {
//        return track switch
//        {
//            ResearchTrack.InformationTechnology => "تقنية المعلومات",
//            ResearchTrack.InformationSecurity => "أمن المعلومات",
//            ResearchTrack.ArtificialIntelligence => "الذكاء الاصطناعي",
//            ResearchTrack.DataScience => "علوم البيانات",
//            ResearchTrack.SoftwareEngineering => "هندسة البرمجيات",
//            _ => track.ToString()
//        };
//    }

//    private string GetDecisionDisplayName(ReviewDecision decision)
//    {
//        return decision switch
//        {
//            ReviewDecision.AcceptAsIs => "قبول فوري",
//            ReviewDecision.AcceptWithMinorRevisions => "قبول مع تعديلات طفيفة",
//            ReviewDecision.MajorRevisionsRequired => "تعديلات جوهرية مطلوبة",
//            ReviewDecision.Reject => "رفض",
//            ReviewDecision.NotSuitableForConference => "غير مناسب للمؤتمر",
//            _ => "لم يتم المراجعة بعد"
//        };
//    }

//    private string GetStatusMessage(ResearchStatus status)
//    {
//        return status switch
//        {
//            ResearchStatus.UnderReview => "<p style='color: #17a2b8;'>بحثكم الآن قيد المراجعة من قبل المختصين.</p>",
//            ResearchStatus.RequiresMinorRevisions => "<p style='color: #ffc107;'>يرجى إجراء التعديلات الطفيفة المطلوبة وإعادة إرسال البحث.</p>",
//            ResearchStatus.RequiresMajorRevisions => "<p style='color: #fd7e14;'>يرجى إجراء التعديلات الجوهرية المطلوبة وإعادة إرسال البحث.</p>",
//            ResearchStatus.Accepted => "<p style='color: #28a745;'>مبروك! تم قبول بحثكم للنشر في المؤتمر.</p>",
//            ResearchStatus.Rejected => "<p style='color: #dc3545;'>نأسف لإبلاغكم أنه تم رفض البحث. يمكنكم مراجعة التعليقات للتحسين.</p>",
//            _ => ""
//        };
//    }
//}

//public class EmailSettings
//{
//    public string SmtpServer { get; set; } = string.Empty;
//    public int SmtpPort { get; set; }
//    public bool EnableSsl { get; set; }
//    public string Username { get; set; } = string.Empty;
//    public string Password { get; set; } = string.Empty;
//    public string FromEmail { get; set; } = string.Empty;
//    public string FromName { get; set; } = string.Empty;
//}
//}
