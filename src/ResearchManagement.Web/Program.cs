using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Infrastructure.Data;
using ResearchManagement.Infrastructure.Repositories;
using ResearchManagement.Infrastructure.Services;
using ResearchManagement.Application.Interfaces;
using System.Reflection;
using MediatR;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// ≈÷«›… «·Œœ„«  ··Õ«ÊÌ
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity Configuration
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // ≈⁄œ«œ«  ﬂ·„… «·„—Ê—
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;

    // ≈⁄œ«œ«  «·ﬁ›·
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // ≈⁄œ«œ«  «·„” Œœ„
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; //  €ÌÌ— ≈·Ï true ›Ì «·≈‰ «Ã
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ≈÷«›… MediatR -  ÕœÌœ Assembly «·’ÕÌÕ
var applicationAssembly = Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory, "ResearchManagement.Application.dll"));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

// ≈÷«›… AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Repository Pattern -  ”ÃÌ· Ã„Ì⁄ «·‹ Repositories
builder.Services.AddScoped<IResearchRepository, ResearchRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IResearchStatusHistoryRepository, ResearchStatusHistoryRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFileService, FileService>();

// ≈⁄œ«œ«  «·»—Ìœ «·≈·ﬂ —Ê‰Ì
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// ≈⁄œ«œ«  —›⁄ «·„·›« 
builder.Services.Configure<FileUploadSettings>(
    builder.Configuration.GetSection("FileUploadSettings"));

// ≈÷«›… Œœ„«  MVC
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute>();
});

// ≈⁄œ«œ«  «·Ã·”…
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ≈⁄œ«œ«  «·ﬂÊﬂÌ“
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// ≈⁄œ«œ«  «· ÊÿÌ‰
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("ar-SA"),
        new CultureInfo("en-US")
    };

    options.DefaultRequestCulture = new RequestCulture("ar-SA");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// ≈÷«›… Œœ„«  ≈÷«›Ì…
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

// Œœ„«  «·Œ·›Ì… («Œ Ì«—Ì… - Ì„ﬂ‰  ⁄ÿÌ·Â« „ƒﬁ «)
builder.Services.AddHostedService<EmailBackgroundService>();
builder.Services.AddHostedService<DeadlineReminderService>();

var app = builder.Build();

//  ﬂÊÌ‰ „”«— ÿ·»«  HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// «· ÊÿÌ‰
app.UseRequestLocalization();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

//  ﬂÊÌ‰ «·„”«—« 
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//  ÂÌ∆… ﬁ«⁄œ… «·»Ì«‰« 
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await DatabaseSeeder.SeedAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();