using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebWorkNew.Data;
using WebWorkNew.Models;
using WebWorkNew.Services;
using WebWorkNew.Enums;
using X.PagedList;

namespace WebWorkNew.Controllers;

[Authorize]
public class ProjectsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IProjectCalculationService _calc;

    public ProjectsController(AppDbContext db, IProjectCalculationService calc)
    {
        _db = db;
        _calc = calc;
    }

    public async Task<IActionResult> Index(string? search, int? page, int pageSize = 10)
    {
        var query = _db.Projects
            .Include(p => p.Customer)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.Title.Contains(search) ||
                (p.Description != null && p.Description.Contains(search)) ||
                (p.Customer != null && p.Customer.FullName.Contains(search)));

            ViewBag.CurrentSearch = search;
        }

        query = query.OrderByDescending(p => p.Id);

        var pageNumber = page ?? 1;
        var paged = await query.ToPagedListAsync(pageNumber, pageSize);

        ViewBag.CurrentPageSize = pageSize;
        return View(paged);
    }


    [HttpGet]
    public async Task<IActionResult> Create(int? workspaceId)
    {
        ViewBag.Customers = await _db.Customers.ToListAsync();
        return View(new Project
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(30),
            TaxRate = 20,
            WorkspaceId = workspaceId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] Project model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Customers = await _db.Customers.ToListAsync();
            return View(model);
        }

        // Подстрахуемся от ситуаций, когда Date/Number не распарсились binder'ом.
        // В этом проекте даты приходят из <input type="date">, а парсинг DateTime может зависеть от культуры.
        // Важно: не добавляем ошибок в ModelState, чтобы не ломать сохранение.
        if (model.StartDate == default)
        {
            var postedStart = Request?.Form["StartDate"].ToString();
            if (!string.IsNullOrWhiteSpace(postedStart) && DateTime.TryParseExact(
                    postedStart,
                    "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var parsedStart))
            {
                model.StartDate = parsedStart;
            }
        }

        if (model.EndDate == default)
        {
            var postedEnd = Request?.Form["EndDate"].ToString();
            if (!string.IsNullOrWhiteSpace(postedEnd) && DateTime.TryParseExact(
                    postedEnd,
                    "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var parsedEnd))
            {
                model.EndDate = parsedEnd;
            }
        }


        // TaxRate/Status берём как пришли binder'ом (если они не распарсились — это уже отражено в ModelState).

        _db.Projects.Add(model);

        await _db.SaveChangesAsync();

        // Важно: повторно загружаем проект из БД (с Customer), чтобы гарантировать корректные данные для списка проектов
        var loadedProject = await _db.Projects
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == model.Id);

        if (loadedProject == null)
            return NotFound();

        await _calc.RecalculateAsync(loadedProject);
        await _db.SaveChangesAsync();


        // После создания возвращаем на страницу списка проектов
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var project = await _db.Projects
            .Include(p => p.Resources)
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (project == null) return NotFound();
        
        ViewBag.Customers = await _db.Customers.ToListAsync();
        return View(project);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Project model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            ViewBag.Customers = await _db.Customers.ToListAsync();
            return View(model);
        }

        var project = await _db.Projects
            .Include(p => p.Resources)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (project == null) return NotFound();

        project.Title = model.Title;
        project.StartDate = model.StartDate;
        project.EndDate = model.EndDate;
        project.Description = model.Description;
        project.CustomerId = model.CustomerId;
        project.TaxRate = model.TaxRate;

        await _calc.RecalculateAsync(project);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var project = await _db.Projects
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (project == null) return NotFound();
        return View(project);
    }
    [HttpGet]
    public async Task<IActionResult> Calculation(int id)
    {
        var project = await _db.Projects
            .Include(p => p.Resources)
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (project == null) return NotFound();
        
        // Пересчитываем для актуальности
        await _calc.RecalculateAsync(project);
        await _db.SaveChangesAsync();
        
        return View(project);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var project = await _db.Projects
            .Include(p => p.Resources)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (project != null)
        {
            _db.ProjectResources.RemoveRange(project.Resources);
            _db.Projects.Remove(project);
            await _db.SaveChangesAsync();
        }
        
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Resources(int id)
    {
        var project = await _db.Projects
            .Include(p => p.Resources)
                .ThenInclude(r => r.Employee)
            .Include(p => p.Resources)
                .ThenInclude(r => r.Executor)
            .Include(p => p.Resources)
                .ThenInclude(r => r.Subcontractor)
            .Include(p => p.Resources)
                .ThenInclude(r => r.Equipment)
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null) return NotFound();

        ViewBag.CanEditMargin = User.IsInRole("CommercialDirector") || User.IsInRole("GlobalAdmin");
        ViewBag.Employees = await _db.Employees.ToListAsync();
        ViewBag.Executors = await _db.Executors.ToListAsync();
        ViewBag.Subcontractors = await _db.Subcontractors.ToListAsync();
        ViewBag.Equipments = await _db.Equipments.ToListAsync();

        return View(project);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddResource(int projectId, ProjectResource resource)
    {
        var project = await _db.Projects
            .Include(p => p.Resources)
            .FirstOrDefaultAsync(p => p.Id == projectId);
            
        if (project == null) 
        return NotFound();


        resource.ProjectId = projectId;
        resource.StartDate = project.StartDate;
        resource.EndDate = project.EndDate;

        // Auto-fill service name if empty
        if (string.IsNullOrWhiteSpace(resource.ServiceName))
        {
            resource.ServiceName = resource.Type switch
            {
                ResourceType.Employee when resource.EmployeeId.HasValue =>
                    (await _db.Employees.FindAsync(resource.EmployeeId.Value))?.Position ?? "Услуга",
                ResourceType.Executor when resource.ExecutorId.HasValue =>
                    (await _db.Executors.FindAsync(resource.ExecutorId.Value))?.FullName ?? "Услуга",
                ResourceType.Subcontractor when resource.SubcontractorId.HasValue =>
                    (await _db.Subcontractors.FindAsync(resource.SubcontractorId.Value))?.Name ?? "Услуга",
                ResourceType.Equipment when resource.EquipmentId.HasValue =>
                    (await _db.Equipments.FindAsync(resource.EquipmentId.Value))?.Title ?? "Услуга",
                _ => "Услуга"
            };
        }

        if (resource.UnitsCount <= 0)
            resource.UnitsCount = 1;

        project.Resources.Add(resource);
        await _calc.RecalculateAsync(project);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Resources), new { id = projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveResource(int projectId, int resourceId)
    {
        var resource = await _db.ProjectResources.FindAsync(resourceId);
        if (resource != null && resource.ProjectId == projectId)
        {
            _db.ProjectResources.Remove(resource);
            
            var project = await _db.Projects
                .Include(p => p.Resources)
                .FirstOrDefaultAsync(p => p.Id == projectId);
                
            if (project != null)
            {
                await _calc.RecalculateAsync(project);
            }
            
            await _db.SaveChangesAsync();
        }
        
        return RedirectToAction(nameof(Resources), new { id = projectId });
    }

    // API для статистики
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var projects = await _db.Projects.ToListAsync();
        var resources = await _db.ProjectResources.ToListAsync();
        
        var stats = new
        {
            projectCount = projects.Count,
            resourceCount = resources.Count,
            totalCost = projects.Sum(p => p.TotalCostWithMargin)
        };
        
        return Ok(stats);
    }
}