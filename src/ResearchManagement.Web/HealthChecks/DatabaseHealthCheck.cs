using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using ResearchManagement.Infrastructure.Data;

namespace ResearchManagement.Web.HealthChecks
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseHealthCheck> _logger;

        public DatabaseHealthCheck(ApplicationDbContext context, ILogger<DatabaseHealthCheck> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // محاولة الاتصال بقاعدة البيانات
                await _context.Database.CanConnectAsync(cancellationToken);

                // محاولة قراءة بيانات بسيطة
                var userCount = await _context.Users.CountAsync(cancellationToken);

                _logger.LogInformation("فحص قاعدة البيانات ناجح - عدد المستخدمين: {UserCount}", userCount);

                return HealthCheckResult.Healthy($"قاعدة البيانات تعمل بشكل صحيح. عدد المستخدمين: {userCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل فحص قاعدة البيانات");

                return HealthCheckResult.Unhealthy(
                    "لا يمكن الاتصال بقاعدة البيانات",
                    ex,
                    new Dictionary<string, object>
                    {
                        ["Error"] = ex.Message,
                        ["ConnectionString"] = _context.Database.GetConnectionString() ?? "غير محدد"
                    });
            }
        }
    }

    // Extension method لإضافة الفحص
    public static class HealthCheckExtensions
    {
        public static IServiceCollection AddDatabaseHealthCheck(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "db", "sql" });

            return services;
        }
    }
}