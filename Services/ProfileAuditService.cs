using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using WebWorkNew.Data;
using WebWorkNew.Models;

namespace WebWorkNew.Services;

public class ProfileAuditService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProfileAuditService(AppDbContext db, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    private string? GetCurrentUserId() => _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    private string? GetCurrentUserEmail()
    {
        var email = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (!string.IsNullOrWhiteSpace(email)) return email;
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    }

    public async Task LogAsync(string action, string entity, int? entityId = null, string? oldValue = null, string? newValue = null)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return;

        var email = GetCurrentUserEmail();
        email ??= (await _userManager.FindByIdAsync(userId))?.Email ?? "";

        var log = new UserAuditLog
        {
            UserId = userId,
            UserEmail = email,
            Action = action,
            Entity = entity,
            EntityId = entityId,
            OldValue = oldValue,
            NewValue = newValue,
            CreatedAt = DateTime.UtcNow
        };

        _db.UserAuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}

