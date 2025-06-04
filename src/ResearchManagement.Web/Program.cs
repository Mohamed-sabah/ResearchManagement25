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

// ����� Serilog ��� Logging �������
builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day));

// ����� DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ����� Identity �� ������� ������
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // ������� ���� ������
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // ������� ��������
    options.User.RequireUniqueEmail = true;

    // ������� �����
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // ������� ����� ������ ����������
    options.SignIn.RequireConfirmedEmail = false; // ���� ������ ��� true �� �������
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ����� Cookie ������
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

// ����� Repositories
builder.Services.AddScoped<IResearchRepository, ResearchRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IResearchStatusHistoryRepository, ResearchStatusHistoryRepository>();

// ����� Services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFileService, FileService>();

// ����� MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ResearchManagement.Application.Commands.Research.CreateResearchCommand).Assembly));

// ����� AutoMapper
builder.Services.AddAutoMapper(typeof(ResearchManagement.Application.Mappings.MappingProfile));

// ����� FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(ResearchManagement.Application.Validators.CreateResearchDtoValidator).Assembly);

// ����� Configuration Settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<FileUploadSettings>(builder.Configuration.GetSection("FileUploadSettings"));

// ����� Controllers �� Views
builder.Services.AddControllersWithViews(options =>
{
    // ����� ����� ���� ��� ��� �����
})
.AddRazorRuntimeCompilation(); // ������� ���

// ����� Background Services
builder.Services.AddHostedService<EmailBackgroundService>();
builder.Services.AddHostedService<DeadlineReminderService>();

// ����� ����� ������ ������ �������
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

// ����� Rate Limiting ������� �� �������
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

// ����� HTTP Client Factory ������� ��������
builder.Services.AddHttpClient();

var app = builder.Build();

// ����� HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    // ������� ���� ��� ����� �� �������
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // ��� ������ ������� �� �������
    app.UseDeveloperExceptionPage();
}

// ����� Middleware �������� ������ �������
app.UseGlobalExceptionMiddleware();

// ����� Serilog request logging
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ����� Rate Limiting
app.UseRateLimiter();

// ����� Authentication � Authorization
app.UseAuthentication();
app.UseAuthorization();

// ����� ��������
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .RequireRateLimiting("general");

// ����� Database Seeding ���� ���
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("��� ����� ������� ������ ����� ��������...");

        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // ����� Migrations ��� ��� �����
        if (context.Database.GetPendingMigrations().Any())
        {
            logger.LogInformation("����� Migrations �������...");
            await context.Database.MigrateAsync();
        }

        // ����� Database Seeding
        await DatabaseSeeder.SeedAsync(context, userManager, roleManager);

        logger.LogInformation("�� ����� ������� �����");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "��� ��� ����� ��� ������� �� ����� ����� ��������");

        // �� ���� ������ѡ ���� ����� �������
        if (app.Environment.IsDevelopment())
        {
            throw;
        }

        // �� ������̡ ��� ����� ������
        logger.LogWarning("������� ������� ��� ��� Database Seeding");
    }
}

// ����� ������� ������ �� ���� �������
app.Lifetime.ApplicationStarted.Register(() =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("�� ��� ����� ����� ����� ������ ������� �����");
});

app.Lifetime.ApplicationStopping.Register(() =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("���� ����� ����� ����� ������ �������...");
});

app.Run();