using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Infrastructure.Data;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Infrastructure.Repositories;
using ResearchManagement.Infrastructure.Services;
using ResearchManagement.Infrastructure.Middleware;
using FluentValidation;
using Serilog;
using Microsoft.AspNetCore.RateLimiting;
using ResearchManagement.Web.Extensions;
using ResearchManagement.Application.Commands.Research;
using ResearchManagement.Application.Mappings;
using MediatR;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Queries.Research;
using ResearchManagement.Web.Mappings;


var builder = WebApplication.CreateBuilder(args);

// ≈⁄œ«œ Serilog ··‹ Logging «·„ ﬁœ„
builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day));

// ≈⁄œ«œ DbContext
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// ≈⁄œ«œ Identity „⁄  Õ”Ì‰«  «·√„«‰
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // ≈⁄œ«œ«  ﬂ·„… «·„—Ê—
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // ≈⁄œ«œ«  «·„” Œœ„
    options.User.RequireUniqueEmail = true;

    // ≈⁄œ«œ«  «·ﬁ›·
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // ≈⁄œ«œ«   √ﬂÌœ «·»—Ìœ «·≈·ﬂ —Ê‰Ì
    options.SignIn.RequireConfirmedEmail = false; // Ì„ﬂ‰  €ÌÌ—Â ≈·Ï true ›Ì «·≈‰ «Ã
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
//builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddMappingServices(); //  √ﬂœ „‰ Â–« «·”ÿ—
// ≈⁄œ«œ Cookie ··ÂÊÌ…
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
});


//  ”ÃÌ· Repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IResearchRepository, ResearchRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IResearchStatusHistoryRepository, ResearchStatusHistoryRepository>();




//  ”ÃÌ· Services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFileService, FileService>();


// 4.  ”ÃÌ· «·Œœ„«  «·√Œ—Ï «·„ÿ·Ê»…

builder.Services.AddScoped<IResearchRepository, ResearchRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IResearchStatusHistoryRepository, ResearchStatusHistoryRepository>();




//  ”ÃÌ· MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ResearchManagement.Application.Commands.Research.CreateResearchCommand).Assembly));
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(CreateResearchCommandHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(UpdateResearchCommandHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(DeleteResearchCommandHandler).Assembly);
    builder.Services.AddScoped<IRequestHandler<UpdateResearchStatusCommand, bool>, UpdateResearchStatusCommandHandler>();

});

// 3.  ”ÃÌ· Query Handlers
builder.Services.AddScoped<IRequestHandler<GetResearchByIdQuery, ResearchDto?>, GetResearchByIdQueryHandler>();
builder.Services.AddScoped<IRequestHandler<GetResearchFileByIdQuery, ResearchFileDto?>, GetResearchFileByIdQueryHandler>();
builder.Services.AddScoped<IRequestHandler<GetResearchListQuery, PagedResult<ResearchDto>>, GetResearchListQueryHandler>();




builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// 6. ≈÷«›… AutoMapper
builder.Services.AddAutoMapper(typeof(ApplicationMappingProfile), typeof(WebMappingProfile));

//  ”ÃÌ· AutoMapper
builder.Services.AddAutoMapper(typeof(ResearchManagement.Application.Mappings.ApplicationMappingProfile));

//  ”ÃÌ· FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(ResearchManagement.Application.Validators.CreateResearchDtoValidator).Assembly);

//  ”ÃÌ· Configuration Settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<FileUploadSettings>(builder.Configuration.GetSection("FileUploadSettings"));

//  ”ÃÌ· Controllers „⁄ Views
builder.Services.AddControllersWithViews(options =>
{
    // ≈÷«›… ›·« — ⁄«„… ≈–« ·“„ «·√„—
})
.AddRazorRuntimeCompilation(); // ·· ÿÊÌ— ›ﬁÿ

//  ”ÃÌ· Background Services
builder.Services.AddHostedService<EmailBackgroundService>();
builder.Services.AddHostedService<DeadlineReminderService>();

// ≈÷«›… Œœ„«  ≈÷«›Ì… ··√„«‰ Ê«·√œ«¡
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

// ≈÷«›… Rate Limiting ··Õ„«Ì… „‰ «·ÂÃ„« 
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("general", cfg =>
    {
        cfg.PermitLimit = 100;
        cfg.Window = TimeSpan.FromMinutes(1);
        cfg.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        cfg.QueueLimit = 10;
    });
});

// ≈÷«›… HTTP Client Factory ··Œœ„«  «·Œ«—ÃÌ…
builder.Services.AddHttpClient();

var app = builder.Build();

//  ﬂÊÌ‰ HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    // «” Œœ«„ ’›Õ… Œÿ√ „Œ’’… ›Ì «·≈‰ «Ã
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // ⁄—÷  ›«’Ì· «·√Œÿ«¡ ›Ì «· ÿÊÌ—
    app.UseDeveloperExceptionPage();
}

// ≈÷«›… Middleware ··„⁄«·Ã… «·⁄«„… ··√Œÿ«¡
app.UseGlobalExceptionMiddleware();

// ≈÷«›… Serilog request logging
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ≈÷«›… Rate Limiting
app.UseRateLimiter();

// ≈÷«›… Authentication Ê Authorization
app.UseAuthentication();
app.UseAuthorization();
await SeedDatabase(app);
//  ﬂÊÌ‰ «·„”«—« 
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .RequireRateLimiting("general");

//  ÿ»Ìﬁ Database Seeding »‘ﬂ· ¬„‰
async Task SeedDatabase(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("»œ¡  ‘€Ì· «· ÿ»Ìﬁ Ê ÿ»Ìﬁ ﬁ«⁄œ… «·»Ì«‰« ...");

            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // ≈‰‘«¡ ﬁ«⁄œ… «·»Ì«‰«  ≈–« ·„  ﬂ‰ „ÊÃÊœ…
            await context.Database.EnsureCreatedAsync();

            //  ÿ»Ìﬁ Database Seeding
            await DatabaseSeeder.SeedAsync(context, userManager, roleManager);
            await EnhancedDatabaseSeeder.SeedAsync(context, userManager, roleManager);
            logger.LogInformation(" „  ‘€Ì· «· ÿ»Ìﬁ »‰Ã«Õ");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ÕœÀ Œÿ√ √À‰«¡ »œ¡ «· ‘€Ì· √Ê  ÿ»Ìﬁ ﬁ«⁄œ… «·»Ì«‰« ");

            // ›Ì »Ì∆… «· ÿÊÌ—° Ì„ﬂ‰ ≈Ìﬁ«› «· ÿ»Ìﬁ
            if (app.Environment.IsDevelopment())
            {
                throw;
            }

            // ›Ì «·≈‰ «Ã° ”Ã· «·Œÿ√ Ê«” „—
            logger.LogWarning("-");
        }
    }
}
// ≈÷«›… „⁄·Ê„«  ≈÷«›Ì… ⁄‰ Õ«·… «· ÿ»Ìﬁ
app.Lifetime.ApplicationStarted.Register(() =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("-");
});

app.Lifetime.ApplicationStopping.Register(() =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("-");
});

app.Run();