using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebWork.Data;
using WebWork.Models;
using WebWork.Services;

namespace WebWork.Controllers;

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
        var projects = await _db.Projects.ToListAsync();
        return View(projects);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var project = await _db.Projects.Include(p => p.Resources).FirstOrDefaultAsync(p => p.Id == id);
        if (project == null) return NotFound();
        return View(project);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Project model)
    {
        if (!ModelState.IsValid) return View(model);

        var project = await _db.Projects.Include(p => p.Resources).FirstOrDefaultAsync(p => p.Id == model.Id);
        if (project == null) return NotFound();

        project.Title = model.Title;
        project.StartDate = model.StartDate;
        project.EndDate = model.EndDate;
        project.Description = model.Description;
        project.CustomerId = model.CustomerId;
        project.TaxRate = model.TaxRate;

        await _calc.RecalculateAsync(project);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Edit), new { id = project.Id });
    }

    public async Task<IActionResult> Resources(int id)
    {
        var project = await _db.Projects.Include(p => p.Resources).FirstOrDefaultAsync(p => p.Id == id);
        if (project == null) return NotFound();
        return View(project);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddResource(int projectId, ProjectResource resource)
    {
        var project = await _db.Projects.Include(p => p.Resources).FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null) return NotFound();

        resource.ProjectId = projectId;
        project.Resources.Add(resource);

        await _calc.RecalculateAsync(project);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Resources), new { id = projectId });
    }
}

