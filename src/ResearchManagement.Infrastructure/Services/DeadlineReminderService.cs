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
using ResearchManagement.Application.Interfaces;

namespace ResearchManagement.Infrastructure.Services
{
    public class DeadlineReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DeadlineReminderService> _logger;

        public DeadlineReminderService(
            IServiceProvider serviceProvider,
            ILogger<DeadlineReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckDeadlines();
                    await Task.Delay(TimeSpan.FromHours(12), stoppingToken); // فحص كل 12 ساعة
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطأ في فحص المواعيد النهائية");
                }
            }
        }

        private async Task CheckDeadlines()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var tomorrow = DateTime.UtcNow.AddDays(1).Date;

            // فحص مواعيد المراجعة
            var upcomingReviewDeadlines = await context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Research)
                .Where(r => !r.IsCompleted && r.Deadline.Date == tomorrow)
                .ToListAsync();

            foreach (var review in upcomingReviewDeadlines)
            {
                var subject = "تذكير: موعد انتهاء المراجعة غداً";
                var message = $"ينتهي موعد مراجعة البحث '{review.Research.Title}' غداً. يرجى إكمال المراجعة في أقرب وقت ممكن.";

                await emailService.SendDeadlineReminderAsync(review.ReviewerId, subject, message);
                _logger.LogInformation($"تم إرسال تذكير المراجعة إلى {review.Reviewer.Email}");
            }

            // فحص المواعيد المتأخرة
            var overdueReviews = await context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Research)
                .Where(r => !r.IsCompleted && r.Deadline < DateTime.UtcNow)
                .ToListAsync();

            foreach (var review in overdueReviews)
            {
                var subject = "تنبيه: تأخر في موعد المراجعة";
                var message = $"لقد تجاوز موعد مراجعة البحث '{review.Research.Title}' الموعد النهائي. يرجى إكمال المراجعة فوراً.";

                await emailService.SendDeadlineReminderAsync(review.ReviewerId, subject, message);
            }
        }
    }
}
