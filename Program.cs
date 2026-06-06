using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using WebWork.Data;
using WebWork.Models;
using WebWork.Services;

// Настройка лицензии QuestPDF
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database configuration - используем SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=webwork.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Identity configuration
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 3;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Business services
builder.Services.AddScoped<IProjectCalculationService, ProjectCalculationService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();

var app = builder.Build();

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Создаем БД если её нет
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
app.UseAuthentication();
app.UseAuthorization();

// Route configuration
app.MapGet("/", context => { context.Response.Redirect("/Projects"); return Task.CompletedTask; });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Projects}/{action=Index}/{id?}");
app.MapControllers();
app.MapRazorPages();

app.Run();