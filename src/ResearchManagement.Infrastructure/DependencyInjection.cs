using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Infrastructure.Data;
using ResearchManagement.Infrastructure.Repositories;
using ResearchManagement.Infrastructure.Services;

namespace ResearchManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Repositories
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IResearchRepository, ResearchRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IResearchFileRepository, ResearchFileRepository>();
            services.AddScoped<ITrackManagerRepository, TrackManagerRepository>();
            services.AddScoped<ITrackReviewerRepository, TrackReviewerRepository>();
            services.AddScoped<IResearchStatusHistoryRepository, ResearchStatusHistoryRepository>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Services
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IFileService, FileService>();

            return services;
        }
    }
}