using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebWorkNew.Data;
using WebWorkNew.Models;

namespace WebWorkNew.Services;

public class UserSnapshotHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(10);

    public UserSnapshotHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Snapshot делаем только для "активных" пользователей (у которых было действие за последние 30 минут)
                // и только если с момента их прошлого Snapshot10m прошло >= 10 минут.

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var now = DateTime.UtcNow;
                var minActivityUtc = now.AddMinutes(-30);

                var activeUserIds = await db.UserAuditLogs
                    .Where(l => l.CreatedAt >= minActivityUtc)
                    .Select(l => l.UserId)
                    .Distinct()
                    .ToListAsync(stoppingToken);

                foreach (var userId in activeUserIds)
                {
                    var userEmail = await userManager.Users
                        .Where(u => u.Id == userId)
                        .Select(u => u.Email ?? string.Empty)
                        .FirstOrDefaultAsync(stoppingToken);

                    var lastSnapshot = await db.UserAuditLogs
                        .Where(l => l.UserId == userId && l.Action == "Snapshot10m")
                        .OrderByDescending(l => l.CreatedAt)
                        .Select(l => l.CreatedAt)
                        .FirstOrDefaultAsync(stoppingToken);

                    // если lastSnapshot == default(DateTime) то snapshot еще не было
                    if (lastSnapshot != default && (now - lastSnapshot) < _interval)
                        continue;

                    db.UserAuditLogs.Add(new UserAuditLog
                    {
                        UserId = userId,
                        UserEmail = userEmail,
                        Action = "Snapshot10m",
                        Entity = "User",
                        EntityId = null,
                        OldValue = null,
                        NewValue = null,
                        CreatedAt = now
                    });
                }


                await db.SaveChangesAsync(stoppingToken);
            }
            catch
            {
                // чтобы не ломать цикл
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (TaskCanceledException) { }
        }
    }
}

