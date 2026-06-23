using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebWorkNew.Data;
using WebWorkNew.Models;

namespace WebWorkNew.Controllers;

[AllowAnonymous]
public class RegisterController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _db;

    public RegisterController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Register()
    {
        // ====== Mini-Captcha (2 шага) ======
        // правила: шаг1 a+b, шаг2 step1+c. Сервер хранит только expected(step2) в Session.
        var a = Random.Shared.Next(1, 10);
        var b = Random.Shared.Next(1, 10);
        var c = Random.Shared.Next(1, 10);
        var expected = (a + b) + c;
        HttpContext.Session.SetString("CaptchaExpected", expected.ToString());

        // Убеждаемся, что роли существуют
        string[] roles = { "GlobalAdmin", "CommercialDirector", "Accountant", "HR" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();
        ViewBag.Roles = allRoles;
        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // Убеждаемся, что роли существуют
        string[] roles = { "GlobalAdmin", "CommercialDirector", "Accountant", "HR" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();
        ViewBag.Roles = allRoles;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // ====== Mini-Captcha (2 шага) ======
        // Шаг 2 проверяем по ожидаемому значению, которое записали на GET.
        var expected = HttpContext.Session.GetString("CaptchaExpected") ?? "";
        var provided = model.CaptchaAnswer?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(expected) || !string.Equals(expected, provided, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "Капча неверная. Повторите попытку.");
            ViewBag.Roles = allRoles;
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
            // Важно: показываем ВСЕ ошибки Identity, включая ошибки пароля.
            foreach (var error in result.Errors)
            {
                // Добавляем и в ModelState (для стандартного вывода)
                ModelState.AddModelError(string.Empty, error.Description);
                // И дополнительно — чтобы было видно, что именно вернул Identity
                ViewBag.IdentityErrors ??= new List<string>();
                ViewBag.IdentityErrors.Add(error.Description);
            }

            // Диагностика: длина пароля, который реально пришёл на сервер
            ViewBag.PasswordLength = model.Password?.Length ?? 0;

            // Диагностика капчи
            ViewBag.CaptchaExpected = HttpContext.Session.GetString("CaptchaExpected") ?? "";
            ViewBag.CaptchaProvided = model.CaptchaAnswer ?? "";

            return View(model);
        }


        // Назначаем роль
        var allowedRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "HR",
            "Accountant"
        };

        if (!string.IsNullOrWhiteSpace(model.Role) && 
            await _roleManager.RoleExistsAsync(model.Role) &&
            allowedRoles.Contains(model.Role))
        {
            await _userManager.AddToRoleAsync(user, model.Role);
        }
        else
        {
            await _userManager.AddToRoleAsync(user, "HR");
        }

        // Аудит
        _db.UserAuditLogs.Add(new UserAuditLog
        {
            UserId = user.Id,
            UserEmail = user.Email ?? string.Empty,
            Action = "Register",
            Entity = "User",
            EntityId = null,
            OldValue = null,
            NewValue = user.Email,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        // АВТОМАТИЧЕСКИЙ ВХОД
        await _signInManager.SignInAsync(user, isPersistent: false);
        await _signInManager.RefreshSignInAsync(user);

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

    // Mini-Captcha
    public string? CaptchaAnswer { get; set; }
}


