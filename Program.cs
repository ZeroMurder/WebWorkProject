using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using WebWorkNew.Data;
using WebWorkNew.Models;
using WebWorkNew.Services;

// Настройка лицензии QuestPDF
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Database configuration - используем SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=WebWorkNew.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Identity configuration
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        
                // НАСТРОЙКИ ПАРОЛЯ - ПРОСТЫЕ
        options.Password.RequireDigit = false;              // Не требует цифры
        options.Password.RequiredLength = 4;                // Минимум 4 символа
        options.Password.RequiredUniqueChars = 0;            // Отключаем требование уникальных символов

        options.Password.RequireNonAlphanumeric = false;    // Не требует спецсимволы
        options.Password.RequireUppercase = false;          // Не требует заглавные
        options.Password.RequireLowercase = false;          // Не требует строчные
        
        // НАСТРОЙКИ ПОЛЬЗОВАТЕЛЯ
        options.User.RequireUniqueEmail = true;             // Email должен быть уникальным
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Business services
builder.Services.AddScoped<IProjectCalculationService, ProjectCalculationService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<ITechnicalTaskService, TechnicalTaskService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();

// Profil audit / snapshots
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ProfileAuditService>();
builder.Services.AddHostedService<UserSnapshotHostedService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();



// Database initialization
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        await db.Database.EnsureCreatedAsync();
        logger.LogInformation("Database created/ensured");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database creation failed");
    }

    // Seed test data
    try
    {
        await TestDataSeeder.InitializeAsync(services);
        logger.LogInformation("Test data seeded successfully");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Test data seeding failed");
    }
    
    // Seed Identity roles and admin user
    try
    {
        await IdentitySeeder.InitializeAsync(services);
        logger.LogInformation("Identity seeded successfully");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Identity seeding failed");
    }

    // Seed Holidays - ДОБАВЛЯЕМ ЭТОТ БЛОК
    try
    {
        HolidaySeeder.Seed(db);
        logger.LogInformation("Holidays seeded successfully");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Holidays seeding failed");
    }
}

// Development middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseAuthorization();

// Route configuration
app.MapGet("/", context => { context.Response.Redirect("/Projects"); return Task.CompletedTask; });

// Важно: по умолчанию маршрутизация должна обслуживать MVC.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Projects}/{action=Index}/{id?}");

app.MapControllers();
app.MapRazorPages();

// Технически важно для Razor views: вернуть 404 если экшн не найден.
// (если при этом показывается пустая страница — значит рендеринг не вызывается)
app.UseStatusCodePages();

// Без MVC-authorize редиректов: экспортные endpoints
// (для соответствия ТЗ требуется реальная отдача файлов форматами)
app.MapGet("/TechnicalTasks/export-pdf", async (int projectId, WebWorkNew.Services.ITechnicalTaskService service) =>
{
    var technicalTask = await service.GetByProjectIdAsync(projectId);
    if (technicalTask == null) return Results.NotFound();

    var pdf = await service.GeneratePdfAsync(technicalTask);
    return Results.File(pdf, "application/pdf", $"ТЗ_проект_{projectId}.pdf");
});

app.MapGet("/TechnicalTasks/export-word", async (int projectId, WebWorkNew.Services.ITechnicalTaskService service) =>
{
    var technicalTask = await service.GetByProjectIdAsync(projectId);
    if (technicalTask == null) return Results.NotFound();

    var bytes = await service.GenerateWordAsync(technicalTask);
    return Results.File(
        bytes,
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        $"ТЗ_проект_{projectId}.docx");
});

app.Run();
