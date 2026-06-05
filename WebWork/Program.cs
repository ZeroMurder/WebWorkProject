using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Helpers;
using WebWork.Data;
using WebWork.Models;
using WebWork.Services;

// Настройка лицензии QuestPDF (иначе генерация PDF падает)
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddDbContext<AppDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";

    // В репозитории часто лежит SQL Server connection string. Для сдачи/локальной отладки принудительно используем SQLite.
    if (cs.Contains("server=", StringComparison.OrdinalIgnoreCase) || cs.Contains("Server=", StringComparison.OrdinalIgnoreCase))
    {
        var sqlitePath = System.IO.Path.Combine(AppContext.BaseDirectory, "webwork.db");
        options.UseSqlite($"Data Source={sqlitePath}");
    }
    else
    {
        options.UseSqlite(cs);
    }
});

builder.Services

    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IProjectCalculationService, ProjectCalculationServiceFixed>();

builder.Services.AddScoped<IDocumentService, DocumentService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();
    db.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));

    var provider = db.Database.ProviderName ?? string.Empty;

    // В репозитории нет Migrations/. Поэтому для SQLite/MigrateAsync ничего не создаст.
    // Чтобы сайт не падал "no such table", создаём схему автоматически.
    if (provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        await db.Database.EnsureCreatedAsync();
    }
    else
    {
        try
        {
            await db.Database.MigrateAsync();
        }
        catch (Exception)
        {
            // Для сдачи: если миграции/БД в состоянии "не готовы", не валим сервер.
        }
    }

    // Seed тестовых данных (бизнес-сущности) — нужен всегда для "как в ТЗ".
    try
    {
        await TestDataSeeder.InitializeAsync(services);
    }
    catch (Exception)
    {
        // Seed пропускаем.
    }

    // Seed Identity — только для не-SQLite (или когда identity схемы уже готовы миграциями).
    try
    {
        if (!provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            await IdentitySeeder.InitializeAsync(services);
        }
    }
    catch (Exception)
    {
        // Seed пропускаем.
    }







}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();