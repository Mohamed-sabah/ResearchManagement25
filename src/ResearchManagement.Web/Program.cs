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

var builder = WebApplication.CreateBuilder(args);

// ≈⁄œ«œ Serilog ··‹ Logging «·„ ﬁœ„
builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day));

// ≈⁄œ«œ DbContext
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
builder.Services.AddScoped<IResearchRepository, ResearchRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IResearchStatusHistoryRepository, ResearchStatusHistoryRepository>();

//  ”ÃÌ· Services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFileService, FileService>();

//  ”ÃÌ· MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ResearchManagement.Application.Commands.Research.CreateResearchCommand).Assembly));

//  ”ÃÌ· AutoMapper
builder.Services.AddAutoMapper(typeof(ResearchManagement.Application.Mappings.MappingProfile));

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

//  ﬂÊÌ‰ «·„”«—« 
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .RequireRateLimiting("general");

//  ÿ»Ìﬁ Database Seeding »‘ﬂ· ¬„‰
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

        //  ÿ»Ìﬁ Migrations ≈–« ·“„ «·√„—
        if (context.Database.GetPendingMigrations().Any())
        {
            logger.LogInformation(" ÿ»Ìﬁ Migrations «·„⁄·ﬁ…...");
            await context.Database.MigrateAsync();
        }

        //  ÿ»Ìﬁ Database Seeding
        await DatabaseSeeder.SeedAsync(context, userManager, roleManager);

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
        logger.LogWarning("«” „—«— «· ‘€Ì· —€„ ›‘· Database Seeding");
    }
}

// ≈÷«›… „⁄·Ê„«  ≈÷«›Ì… ⁄‰ Õ«·… «· ÿ»Ìﬁ
app.Lifetime.ApplicationStarted.Register(() =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation(" „ »œ¡  ‘€Ì·  ÿ»Ìﬁ ≈œ«—… «·»ÕÊÀ «·⁄·„Ì… »‰Ã«Õ");
});

app.Lifetime.ApplicationStopping.Register(() =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Ã«—Ì ≈Ìﬁ«›  ÿ»Ìﬁ ≈œ«—… «·»ÕÊÀ «·⁄·„Ì…...");
});

app.Run();