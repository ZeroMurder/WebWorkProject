using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebWorkNew.Data;
using WebWorkNew.Models;

namespace WebWorkNew.Controllers;

[Authorize]
public class ProfilesController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfilesController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction("Login", "Account");

        var avatar = await _db.UserAvatars.FirstOrDefaultAsync(a => a.UserId == userId);
        ViewBag.AvatarPath = avatar?.AvatarPath;

        var logs = await _db.UserAuditLogs
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .Take(50)
            .ToListAsync();
        ViewBag.AuditLogs = logs;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAvatar(IFormFile avatar)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        if (avatar == null || avatar.Length == 0)
            return RedirectToAction(nameof(Index));

        // ограничения
        var allowedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".gif"
        };

        var ext = Path.GetExtension(avatar.FileName);
        if (!allowedExt.Contains(ext))
            return RedirectToAction(nameof(Index));

        const long maxBytes = 2 * 1024 * 1024; // 2MB
        if (avatar.Length > maxBytes)
            return RedirectToAction(nameof(Index));

        var avatarsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
        Directory.CreateDirectory(avatarsDir);

        var fileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
        var filePath = Path.Combine(avatarsDir, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await avatar.CopyToAsync(stream);
        }

        var avatarPath = $"/uploads/avatars/{fileName}";

        var existing = await _db.UserAvatars.FirstOrDefaultAsync(a => a.UserId == userId);
        if (existing == null)
        {
            _db.UserAvatars.Add(new UserAvatar { UserId = userId, AvatarPath = avatarPath });
        }
        else
        {
            existing.AvatarPath = avatarPath;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        _db.UserAuditLogs.Add(new UserAuditLog
        {
            UserId = userId,
            UserEmail = (await _userManager.FindByIdAsync(userId))?.Email ?? "",
            Action = "AvatarUpload",
            Entity = "UserAvatar",
            EntityId = null,
            OldValue = null,
            NewValue = avatarPath,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}

