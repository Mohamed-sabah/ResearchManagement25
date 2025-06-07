using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Infrastructure.Data
{
   
        public static class DatabaseSeeder
        {
            public static async Task SeedAsync(
                ApplicationDbContext context,
                UserManager<User> userManager,
                RoleManager<IdentityRole> roleManager)
            {
                // تم إنشاء قاعدة البيانات مسبقاً

                // إنشاء الأدوار
                await SeedRolesAsync(roleManager);

                // إنشاء المستخدم الإداري
                await SeedAdminUserAsync(userManager);

                // إنشاء بيانات تجريبية
                await SeedSampleDataAsync(context, userManager);
            }

            private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
            {
                var roles = new[]
                {
                "Researcher",
                "Reviewer",
                "TrackManager",
                "ConferenceManager",
                "SystemAdmin"
            };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
            }

            private static async Task SeedAdminUserAsync(UserManager<User> userManager)
            {
                var adminEmail = "admin@conference.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    adminUser = new User
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FirstName = "مدير",
                        LastName = "النظام",
                        FirstNameEn = "System",
                        LastNameEn = "Administrator",
                        Role = UserRole.SystemAdmin,
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "SystemAdmin");
                    }
                }
            }

            private static async Task SeedSampleDataAsync(ApplicationDbContext context, UserManager<User> userManager)
            {
                // التحقق من وجود بيانات
                if (await context.Users.AnyAsync(u => u.Role != UserRole.SystemAdmin))
                    return;

                // إنشاء مستخدمين تجريبيين
                var researcher = new User
                {
                    UserName = "researcher@test.com",
                    Email = "researcher@test.com",
                    FirstName = "أحمد",
                    LastName = "محمد",
                    FirstNameEn = "Ahmed",
                    LastNameEn = "Mohammed",
                    Institution = "جامعة بغداد",
                    AcademicDegree = "دكتوراه",
                    Role = UserRole.Researcher,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await userManager.CreateAsync(researcher, "Test@123456");
                await userManager.AddToRoleAsync(researcher, "Researcher");

                var reviewer = new User
                {
                    UserName = "reviewer@test.com",
                    Email = "reviewer@test.com",
                    FirstName = "فاطمة",
                    LastName = "علي",
                    FirstNameEn = "Fatima",
                    LastNameEn = "Ali",
                    Institution = "الجامعة التكنولوجية",
                    AcademicDegree = "أستاذ مساعد",
                    Role = UserRole.Reviewer,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await userManager.CreateAsync(reviewer, "Test@123456");
                await userManager.AddToRoleAsync(reviewer, "Reviewer");

                var trackManager = new User
                {
                    UserName = "trackmanager@test.com",
                    Email = "trackmanager@test.com",
                    FirstName = "خالد",
                    LastName = "حسن",
                    FirstNameEn = "Khalid",
                    LastNameEn = "Hassan",
                    Institution = "جامعة بابل",
                    AcademicDegree = "أستاذ",
                    Role = UserRole.TrackManager,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await userManager.CreateAsync(trackManager, "Test@123456");
                await userManager.AddToRoleAsync(trackManager, "TrackManager");

                await context.SaveChangesAsync();

                // إنشاء TrackManager entity
                var trackManagerEntity = new TrackManager
                {
                    UserId = trackManager.Id,
                    Track = ResearchTrack.InformationTechnology,
                    TrackDescription = "تقنية المعلومات والحوسبة",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.TrackManagers.Add(trackManagerEntity);

                // إضافة المراجع للتراك
                var trackReviewer = new TrackReviewer
                {
                    TrackManagerId = trackManagerEntity.Id,
                    ReviewerId = reviewer.Id,
                    Track = ResearchTrack.InformationTechnology,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.TrackReviewers.Add(trackReviewer);

                await context.SaveChangesAsync();
            }
        }
    }
    
