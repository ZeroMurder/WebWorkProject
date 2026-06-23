using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebWorkNew.Data;
using WebWorkNew.Models;
using WebWorkNew.Services;

namespace WebWorkNew.Controllers;

[Authorize]
public class TechnicalTasksController : Controller
{
    private readonly AppDbContext _db;
    private readonly ITechnicalTaskService _service;

    public TechnicalTasksController(AppDbContext db, ITechnicalTaskService service)
    {
        _db = db;
        _service = service;
    }

    [HttpGet("edit")]
    public async Task<IActionResult> Edit(int projectId)


    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null) return NotFound();

        var technicalTask = await _service.GetByProjectIdAsync(projectId);

        ViewBag.Project = project;
        return View(technicalTask ?? new TechnicalTask { ProjectId = projectId, Status = TechnicalTaskStatus.Draft });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int projectId, TechnicalTask model)
    {
        if (projectId != model.ProjectId) model.ProjectId = projectId;

        if (!ModelState.IsValid)
        {
            var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
            ViewBag.Project = project;
            return View(model);
        }

        if (!await _service.ValidateAsync(model))
        {
            ModelState.AddModelError("", "Некорректные данные ТЗ");
            var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
            ViewBag.Project = project;
            return View(model);
        }

        await _service.CreateOrUpdateAsync(model);
        return RedirectToAction("Index", "TechnicalTasks");
    }

    [HttpGet("export-pdf")]
    public async Task<IActionResult> ExportPdf(int projectId)


    {
        var technicalTask = await _service.GetByProjectIdAsync(projectId);
        if (technicalTask == null) return NotFound();

        // При генерации HTML/PDF сервис повторно подгружает проект по ProjectId.
        var pdf = await _service.GeneratePdfAsync(technicalTask);
        return File(pdf, "application/pdf", $"ТЗ_проект_{projectId}.pdf");
    }

    [AllowAnonymous]
    [HttpGet("export-word")]
    public async Task<IActionResult> ExportWord(int projectId)



    {
        var technicalTask = await _service.GetByProjectIdAsync(projectId);
        if (technicalTask == null) return NotFound();

        var bytes = await _service.GenerateWordAsync(technicalTask);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            $"ТЗ_проект_{projectId}.docx");
    }
}

