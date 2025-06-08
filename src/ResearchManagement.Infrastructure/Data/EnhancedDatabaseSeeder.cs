using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Infrastructure.Data
{
    public static class EnhancedDatabaseSeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Seed roles
            await SeedRolesAsync(roleManager);

            // Seed users
            await SeedUsersAsync(userManager);

            // Seed track managers and relationships
            await SeedTrackManagementAsync(context, userManager);

            // Seed sample research data
            await SeedSampleResearchAsync(context, userManager);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[]
            {
                "Researcher", "Reviewer", "TrackManager",
                "ConferenceManager", "SystemAdmin"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedUsersAsync(UserManager<User> userManager)
        {
            // Admin User
            await CreateUserIfNotExists(userManager, new User
            {
                UserName = "admin@research.com",
                Email = "admin@research.com",
                FirstName = "مدير",
                LastName = "النظام",
                FirstNameEn = "System",
                LastNameEn = "Administrator",
                Role = UserRole.SystemAdmin,
                EmailConfirmed = true,
                IsActive = true
            }, "Admin@123456", "SystemAdmin");

            // Sample Researchers
            await CreateUserIfNotExists(userManager, new User
            {
                UserName = "ahmed.mohammed@research.com",
                Email = "ahmed.mohammed@research.com",
                FirstName = "أحمد",
                LastName = "محمد",
                FirstNameEn = "Ahmed",
                LastNameEn = "Mohammed",
                Institution = "جامعة بغداد",
                AcademicDegree = "دكتوراه",
                Role = UserRole.Researcher,
                EmailConfirmed = true,
                IsActive = true
            }, "Researcher@123", "Researcher");

            await CreateUserIfNotExists(userManager, new User
            {
                UserName = "fatima.ali@research.com",
                Email = "fatima.ali@research.com",
                FirstName = "فاطمة",
                LastName = "علي",
                FirstNameEn = "Fatima",
                LastNameEn = "Ali",
                Institution = "الجامعة التكنولوجية",
                AcademicDegree = "ماجستير",
                Role = UserRole.Researcher,
                EmailConfirmed = true,
                IsActive = true
            }, "Researcher@123", "Researcher");

            // Sample Reviewers
            await CreateUserIfNotExists(userManager, new User
            {
                UserName = "omar.hassan@research.com",
                Email = "omar.hassan@research.com",
                FirstName = "عمر",
                LastName = "حسن",
                FirstNameEn = "Omar",
                LastNameEn = "Hassan",
                Institution = "جامعة البصرة",
                AcademicDegree = "أستاذ مساعد",
                Role = UserRole.Reviewer,
                EmailConfirmed = true,
                IsActive = true
            }, "Reviewer@123", "Reviewer");

            await CreateUserIfNotExists(userManager, new User
            {
                UserName = "sara.ibrahim@research.com",
                Email = "sara.ibrahim@research.com",
                FirstName = "سارة",
                LastName = "إبراهيم",
                FirstNameEn = "Sara",
                LastNameEn = "Ibrahim",
                Institution = "جامعة الموصل",
                AcademicDegree = "أستاذ",
                Role = UserRole.Reviewer,
                EmailConfirmed = true,
                IsActive = true
            }, "Reviewer@123", "Reviewer");

            // Track Manager
            await CreateUserIfNotExists(userManager, new User
            {
                UserName = "khalid.abdullah@research.com",
                Email = "khalid.abdullah@research.com",
                FirstName = "خالد",
                LastName = "عبد الله",
                FirstNameEn = "Khalid",
                LastNameEn = "Abdullah",
                Institution = "جامعة كربلاء",
                AcademicDegree = "أستاذ",
                Role = UserRole.TrackManager,
                EmailConfirmed = true,
                IsActive = true
            }, "TrackManager@123", "TrackManager");
        }

        private static async Task CreateUserIfNotExists(
            UserManager<User> userManager,
            User user,
            string password,
            string role)
        {
            var existingUser = await userManager.FindByEmailAsync(user.Email);
            if (existingUser == null)
            {
                user.CreatedAt = DateTime.UtcNow;
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }

        private static async Task SeedTrackManagementAsync(
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            if (await context.TrackManagers.AnyAsync()) return;

            var trackManager = await userManager.FindByEmailAsync("khalid.abdullah@research.com");
            if (trackManager == null) return;

            // Create Track Manager
            var trackManagerEntity = new TrackManager
            {
                UserId = trackManager.Id,
                Track = ResearchTrack.InformationTechnology,
                TrackDescription = "تقنية المعلومات والحوسبة",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.TrackManagers.Add(trackManagerEntity);
            await context.SaveChangesAsync();

            // Add reviewers to track
            var reviewers = new[]
            {
                "omar.hassan@research.com",
                "sara.ibrahim@research.com"
            };

            foreach (var reviewerEmail in reviewers)
            {
                var reviewer = await userManager.FindByEmailAsync(reviewerEmail);
                if (reviewer != null)
                {
                    var trackReviewer = new TrackReviewer
                    {
                        TrackManagerId = trackManagerEntity.Id,
                        ReviewerId = reviewer.Id,
                        Track = ResearchTrack.InformationTechnology,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.TrackReviewers.Add(trackReviewer);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedSampleResearchAsync(
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            if (await context.Researches.AnyAsync()) return;

            var researcher1 = await userManager.FindByEmailAsync("ahmed.mohammed@research.com");
            var researcher2 = await userManager.FindByEmailAsync("fatima.ali@research.com");

            if (researcher1 == null || researcher2 == null) return;

            // Sample Research 1
            var research1 = new Research
            {
                Title = "تطبيق الذكاء الاصطناعي في أنظمة إدارة قواعد البيانات",
                TitleEn = "Applying AI in Database Management Systems",
                AbstractAr = "يهدف هذا البحث إلى دراسة إمكانيات تطبيق تقنيات الذكاء الاصطناعي لتحسين أداء أنظمة إدارة قواعد البيانات. تم اختبار عدة خوارزميات للتعلم الآلي على قواعد بيانات مختلفة الأحجام. أظهرت النتائج تحسناً ملحوظاً في سرعة الاستعلامات وكفاءة التخزين. كما تم تطوير نموذج تنبؤي لتحسين توزيع البيانات وتقليل وقت الاستجابة.",
                AbstractEn = "This research aims to study the potential of applying artificial intelligence techniques to improve database management system performance. Several machine learning algorithms were tested on databases of different sizes. Results showed significant improvement in query speed and storage efficiency.",
                Keywords = "ذكاء اصطناعي، قواعد بيانات، تعلم آلي، تحسين الأداء",
                KeywordsEn = "artificial intelligence, databases, machine learning, performance optimization",
                ResearchType = ResearchType.ExperimentalStudy,
                Language = ResearchLanguage.Bilingual,
                Track = ResearchTrack.ArtificialIntelligence,
                Status = ResearchStatus.Submitted,
                SubmittedById = researcher1.Id,
                SubmissionDate = DateTime.UtcNow.AddDays(-10),
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                CreatedBy = researcher1.Id
            };

            context.Researches.Add(research1);
            await context.SaveChangesAsync();

            // Add authors for research 1
            var author1 = new ResearchAuthor
            {
                ResearchId = research1.Id,
                FirstName = researcher1.FirstName,
                LastName = researcher1.LastName,
                FirstNameEn = researcher1.FirstNameEn,
                LastNameEn = researcher1.LastNameEn,
                Email = researcher1.Email,
                Institution = researcher1.Institution,
                AcademicDegree = researcher1.AcademicDegree,
                Order = 1,
                IsCorresponding = true,
                UserId = researcher1.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = researcher1.Id
            };

            context.ResearchAuthors.Add(author1);

            // Sample Research 2
            var research2 = new Research
            {
                Title = "أمن الشبكات في البيئات السحابية الهجينة",
                TitleEn = "Network Security in Hybrid Cloud Environments",
                AbstractAr = "يتناول هذا البحث التحديات الأمنية في البيئات السحابية الهجينة ويقترح حلولاً متقدمة لحماية البيانات والشبكات. تم تطوير إطار عمل شامل لإدارة الأمان عبر البيئات المختلطة. النتائج تظهر فعالية النموذج المقترح في تحسين مستوى الأمان بنسبة تصل إلى 85% مقارنة بالطرق التقليدية.",
                AbstractEn = "This research addresses security challenges in hybrid cloud environments and proposes advanced solutions for data and network protection. A comprehensive security management framework was developed for mixed environments.",
                Keywords = "أمن الشبكات، الحوسبة السحابية، البيئات الهجينة، حماية البيانات",
                KeywordsEn = "network security, cloud computing, hybrid environments, data protection",
                ResearchType = ResearchType.AppliedResearch,
                Language = ResearchLanguage.Bilingual,
                Track = ResearchTrack.InformationSecurity,
                Status = ResearchStatus.UnderReview,
                SubmittedById = researcher2.Id,
                SubmissionDate = DateTime.UtcNow.AddDays(-5),
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                CreatedBy = researcher2.Id
            };

            context.Researches.Add(research2);
            await context.SaveChangesAsync();

            // Add authors for research 2
            var author2 = new ResearchAuthor
            {
                ResearchId = research2.Id,
                FirstName = researcher2.FirstName,
                LastName = researcher2.LastName,
                FirstNameEn = researcher2.FirstNameEn,
                LastNameEn = researcher2.LastNameEn,
                Email = researcher2.Email,
                Institution = researcher2.Institution,
                AcademicDegree = researcher2.AcademicDegree,
                Order = 1,
                IsCorresponding = true,
                UserId = researcher2.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = researcher2.Id
            };

            context.ResearchAuthors.Add(author2);
            await context.SaveChangesAsync();
        }
    }
}