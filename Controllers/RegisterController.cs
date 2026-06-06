using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebWork.Models;

namespace WebWork.Controllers;

[AllowAnonymous]
public class RegisterController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public RegisterController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public IActionResult Register()
    {
        ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();

        if (!ModelState.IsValid)
            return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            MiddleName = model.MiddleName,
            Position = model.Position,
            EmailConfirmed = true  // Автоматически подтверждаем для теста
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, err.Description);
            return View(model);
        }

        // Назначаем роль (если указана и существует)
        if (!string.IsNullOrWhiteSpace(model.Role) && await _roleManager.RoleExistsAsync(model.Role))
        {
            await _userManager.AddToRoleAsync(user, model.Role);
        }
        else
        {
            // Роль по умолчанию - HR
            await _userManager.AddToRoleAsync(user, "HR");
        }

        // Автоматически входим после регистрации
        await _signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToAction("Index", "Projects");
    }
}

public class RegisterViewModel
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? MiddleName { get; set; }
    public string Position { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string? Role { get; set; }
}