using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebWorkNew.Data;
using WebWorkNew.Models;

namespace WebWorkNew.Controllers;

[Authorize(Roles = "GlobalAdmin")]
public class AdminUsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _db;

    public AdminUsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }

    // GET: AdminUsers
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        var userRolesMap = new Dictionary<string, IList<string>>();
        
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userRolesMap[user.Id] = roles;
        }
        
        ViewBag.UserRoles = userRolesMap;
        return View(users);
    }

    // GET: AdminUsers/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Roles = await _roleManager.Roles.ToListAsync();
        ViewBag.Workspaces = await _db.Workspaces.ToListAsync();
        return View(new CreateUserViewModel());
    }

    // POST: AdminUsers/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Workspaces = await _db.Workspaces.ToListAsync();
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            MiddleName = model.MiddleName,
            Position = model.Position,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Workspaces = await _db.Workspaces.ToListAsync();
            return View(model);
        }

        // Назначаем роль
        if (!string.IsNullOrWhiteSpace(model.Role))
        {
            await _userManager.AddToRoleAsync(user, model.Role);
        }

        // Назначаем рабочие области
        if (model.WorkspaceIds != null && model.WorkspaceIds.Any())
        {
            foreach (var workspaceId in model.WorkspaceIds)
            {
                var workspaceUser = new WorkspaceUser
                {
                    WorkspaceId = workspaceId,
                    UserId = user.Id,
                    CanView = true,
                    CanEditProjects = model.WorkspacePermissions.ContainsKey(workspaceId) && model.WorkspacePermissions[workspaceId].CanEdit,
                    CanManageWorkspace = model.WorkspacePermissions.ContainsKey(workspaceId) && model.WorkspacePermissions[workspaceId].CanManage
                };
                _db.WorkspaceUsers.Add(workspaceUser);
            }
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: AdminUsers/Edit/5
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        var currentRole = roles.FirstOrDefault() ?? "";
        
        var allRoles = await _roleManager.Roles.ToListAsync();
        var allWorkspaces = await _db.Workspaces.ToListAsync();
        
        var userWorkspaces = await _db.WorkspaceUsers
            .Where(wu => wu.UserId == user.Id)
            .ToDictionaryAsync(wu => wu.WorkspaceId);

        var model = new EditUserViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            MiddleName = user.MiddleName,
            Email = user.Email ?? "",
            Position = user.Position ?? "",
            Role = currentRole,
            WorkspaceIds = userWorkspaces.Keys.ToList(),
            WorkspacePermissions = userWorkspaces.ToDictionary(
                kvp => kvp.Key,
                kvp => new WorkspacePermissionViewModel
                {
                    CanEdit = kvp.Value.CanEditProjects,
                    CanManage = kvp.Value.CanManageWorkspace
                }
            )
        };

        ViewBag.Roles = allRoles;
        ViewBag.Workspaces = allWorkspaces;
        
        return View(model);
    }

    // POST: AdminUsers/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditUserViewModel model)
    {
        if (id != model.Id) return NotFound();
        
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Workspaces = await _db.Workspaces.ToListAsync();
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.MiddleName = model.MiddleName;
        user.Email = model.Email;
        user.UserName = model.Email;
        user.Position = model.Position;

        var updateResult = await _userManager.UpdateAsync(user);
        
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Workspaces = await _db.Workspaces.ToListAsync();
            return View(model);
        }

        // Обновляем роль
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!string.IsNullOrWhiteSpace(model.Role))
        {
            await _userManager.AddToRoleAsync(user, model.Role);
        }

        // Обновляем рабочие области
        var existingWorkspaceUsers = await _db.WorkspaceUsers
            .Where(wu => wu.UserId == user.Id)
            .ToListAsync();
            
        _db.WorkspaceUsers.RemoveRange(existingWorkspaceUsers);
        
        if (model.WorkspaceIds != null && model.WorkspaceIds.Any())
        {
            foreach (var workspaceId in model.WorkspaceIds)
            {
                var workspaceUser = new WorkspaceUser
                {
                    WorkspaceId = workspaceId,
                    UserId = user.Id,
                    CanView = true,
                    CanEditProjects = model.WorkspacePermissions.ContainsKey(workspaceId) && model.WorkspacePermissions[workspaceId].CanEdit,
                    CanManageWorkspace = model.WorkspacePermissions.ContainsKey(workspaceId) && model.WorkspacePermissions[workspaceId].CanManage
                };
                _db.WorkspaceUsers.Add(workspaceUser);
            }
        }
        
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // POST: AdminUsers/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            // Удаляем связи с рабочими областями
            var workspaceUsers = await _db.WorkspaceUsers
                .Where(wu => wu.UserId == user.Id)
                .ToListAsync();
            _db.WorkspaceUsers.RemoveRange(workspaceUsers);
            
            await _db.SaveChangesAsync();
            await _userManager.DeleteAsync(user);
        }
        
        return RedirectToAction(nameof(Index));
    }
}

// ViewModels для AdminUsers
public class CreateUserViewModel
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? MiddleName { get; set; }
    public string Position { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string? Role { get; set; }
    public List<int>? WorkspaceIds { get; set; }
    public Dictionary<int, WorkspacePermissionViewModel> WorkspacePermissions { get; set; } = new();
}

public class EditUserViewModel
{
    public string Id { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? MiddleName { get; set; }
    public string Position { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Role { get; set; }
    public List<int>? WorkspaceIds { get; set; }
    public Dictionary<int, WorkspacePermissionViewModel> WorkspacePermissions { get; set; } = new();
}

public class WorkspacePermissionViewModel
{
    public bool CanEdit { get; set; }
    public bool CanManage { get; set; }
}