using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ResearchManagement.Infrastructure.Data;
using ResearchManagement.Domain.Enums;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;
namespace ResearchManagement.Infrastructure.Services
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailBackgroundService> _logger;
        private readonly EmailSettings _emailSettings;

        public EmailBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<EmailBackgroundService> logger,
            IOptions<EmailSettings> emailSettings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _emailSettings = emailSettings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingEmails();
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // فحص كل 5 دقائق
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطأ في معالجة الإيميلات المعلقة");
                }
            }
        }

        private async Task ProcessPendingEmails()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var pendingEmails = await context.EmailNotifications
                .Where(e => e.Status == NotificationStatus.Pending && e.RetryCount < 3)
                .Take(10)
                .ToListAsync();

            foreach (var email in pendingEmails)
            {
                try
                {
                    await SendEmailAsync(email);
                    email.Status = NotificationStatus.Sent;
                    email.SentAt = DateTime.UtcNow;
                    _logger.LogInformation($"تم إرسال الإيميل بنجاح إلى {email.ToEmail}");
                }
                catch (Exception ex)
                {
                    email.Status = NotificationStatus.Failed;
                    email.ErrorMessage = ex.Message;
                    email.RetryCount++;
                    _logger.LogError(ex, $"فشل في إرسال الإيميل إلى {email.ToEmail}");
                }

                email.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
        }

        private async Task SendEmailAsync(Domain.Entities.EmailNotification emailNotification)
        {
            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
            client.EnableSsl = _emailSettings.EnableSsl;
            client.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = emailNotification.Subject,
                Body = emailNotification.Body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(emailNotification.ToEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}
