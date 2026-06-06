using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebWork.Data;
using WebWork.Models;
using WebWork.Services;
using WebWork.Enums;

namespace WebWork.Controllers;

// [Authorize(Roles = "GlobalAdmin,CommercialDirector")]
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

    public async Task<IActionResult> Index()
    {
        var projects = await _db.Projects
            .Include(p => p.Customer)
            .ToListAsync();
        return View(projects);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Customers = await _db.Customers.ToListAsync();
        return View(new Project
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(30),
            TaxRate = 20
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Project model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Customers = await _db.Customers.ToListAsync();
            return View(model);
        }

        _db.Projects.Add(model);
        await _db.SaveChangesAsync();

        await _calc.RecalculateAsync(model);
        await _db.SaveChangesAsync();

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
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (project == null) return NotFound();
        
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
            
        if (project == null) return NotFound();

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