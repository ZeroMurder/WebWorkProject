using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebWorkNew.Data;
using WebWorkNew.Models;

namespace WebWorkNew.Controllers;

[Authorize]
public class WorkspacesController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public WorkspacesController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // Список рабочих областей, доступных текущему пользователю
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var workspaces = await _db.Workspaces
            .Include(w => w.Users)
            .Where(w => w.AdminUserId == userId || w.Users.Any(u => u.UserId == userId))
            .ToListAsync();
        return View(workspaces);
    }

    [HttpGet]
    [Authorize(Roles = "GlobalAdmin")]
    public IActionResult Create() => View(new Workspace());

    [HttpPost]
    [Authorize(Roles = "GlobalAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Workspace model)
    {
        if (!ModelState.IsValid) return View(model);

        // Проверка уникальности поддомена
        if (await _db.Workspaces.AnyAsync(w => w.Subdomain == model.Subdomain))
        {
            ModelState.AddModelError("Subdomain", "Этот поддомен уже занят");
            return View(model);
        }

        model.AdminUserId = _userManager.GetUserId(User);
        _db.Workspaces.Add(model);
        await _db.SaveChangesAsync();

        // Добавляем создателя как администратора рабочей области с полными правами
        var workspaceUser = new WorkspaceUser
        {
            WorkspaceId = model.Id,
            UserId = model.AdminUserId,
            CanView = true,
            CanEditProjects = true,
            CanManageWorkspace = true
        };
        _db.WorkspaceUsers.Add(workspaceUser);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var workspace = await _db.Workspaces
            .Include(w => w.Users)
            .ThenInclude(wu => wu.User)
            .FirstOrDefaultAsync(w => w.Id == id);
        if (workspace == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        var userRights = workspace.Users.FirstOrDefault(u => u.UserId == userId);
        var canEditProjects = userRights?.CanEditProjects == true || workspace.AdminUserId == userId || User.IsInRole("GlobalAdmin");
        var canManageWorkspace = userRights?.CanManageWorkspace == true || workspace.AdminUserId == userId || User.IsInRole("GlobalAdmin");

        var projects = await _db.Projects
            .Where(p => p.WorkspaceId == id)
            .Include(p => p.Customer)
            .ToListAsync();

        ViewBag.CanEditProjects = canEditProjects;
        ViewBag.CanManageWorkspace = canManageWorkspace;
        ViewBag.Projects = projects;
        return View(workspace);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var workspace = await _db.Workspaces.FindAsync(id);
        if (workspace == null) return NotFound();
        if (!User.IsInRole("GlobalAdmin") && workspace.AdminUserId != _userManager.GetUserId(User))
            return Forbid();
        return View(workspace);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Workspace model)
    {
        if (id != model.Id) return NotFound();
        var workspace = await _db.Workspaces.FindAsync(id);
        if (workspace == null) return NotFound();
        if (!User.IsInRole("GlobalAdmin") && workspace.AdminUserId != _userManager.GetUserId(User))
            return Forbid();

        if (workspace.Subdomain != model.Subdomain &&
            await _db.Workspaces.AnyAsync(w => w.Subdomain == model.Subdomain && w.Id != id))
        {
            ModelState.AddModelError("Subdomain", "Этот поддомен уже занят");
            return View(model);
        }

        workspace.Name = model.Name;
        workspace.Subdomain = model.Subdomain;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // Управление участниками
    [HttpGet]
    public async Task<IActionResult> ManageUsers(int id)
    {
        var workspace = await _db.Workspaces.FindAsync(id);
        if (workspace == null) return NotFound();
        if (!User.IsInRole("GlobalAdmin") && workspace.AdminUserId != _userManager.GetUserId(User))
            return Forbid();

        var users = await _userManager.Users.ToListAsync();
        var workspaceUsers = await _db.WorkspaceUsers
            .Where(wu => wu.WorkspaceId == id)
            .ToDictionaryAsync(wu => wu.UserId);

        ViewBag.Workspace = workspace;
        ViewBag.WorkspaceUsers = workspaceUsers;
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddUser(int workspaceId, string userId, bool canView, bool canEditProjects, bool canManageWorkspace)
    {
        var workspace = await _db.Workspaces.FindAsync(workspaceId);
        if (workspace == null) return NotFound();
        if (!User.IsInRole("GlobalAdmin") && workspace.AdminUserId != _userManager.GetUserId(User))
            return Forbid();

        if (await _db.WorkspaceUsers.AnyAsync(wu => wu.WorkspaceId == workspaceId && wu.UserId == userId))
        {
            TempData["Error"] = "Пользователь уже добавлен в эту рабочую область";
            return RedirectToAction(nameof(ManageUsers), new { id = workspaceId });
        }

        var workspaceUser = new WorkspaceUser
        {
            WorkspaceId = workspaceId,
            UserId = userId,
            CanView = canView,
            CanEditProjects = canEditProjects,
            CanManageWorkspace = canManageWorkspace
        };
        _db.WorkspaceUsers.Add(workspaceUser);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(ManageUsers), new { id = workspaceId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveUser(int workspaceId, int workspaceUserId)
    {
        var workspace = await _db.Workspaces.FindAsync(workspaceId);
        if (workspace == null) return NotFound();
        if (!User.IsInRole("GlobalAdmin") && workspace.AdminUserId != _userManager.GetUserId(User))
            return Forbid();

        var wu = await _db.WorkspaceUsers.FindAsync(workspaceUserId);
        if (wu != null && wu.WorkspaceId == workspaceId)
        {
            _db.WorkspaceUsers.Remove(wu);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(ManageUsers), new { id = workspaceId });
    }

    [HttpPost]
    [Authorize(Roles = "GlobalAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var workspace = await _db.Workspaces.FindAsync(id);
        if (workspace != null)
        {
            _db.Workspaces.Remove(workspace);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}