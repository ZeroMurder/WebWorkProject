using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebWorkNew.Data;
using WebWorkNew.Services;

namespace WebWorkNew.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectApiController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IProjectCalculationService _calc;

    public ProjectApiController(AppDbContext db, IProjectCalculationService calc)
    {
        _db = db;
        _calc = calc;
    }

    public sealed class UpdateMarginRequest
    {
        public int ProjectId { get; set; }
        public int ResourceId { get; set; }
        public decimal MarginPercent { get; set; }
    }

    [HttpPost("update-margin")]
    public async Task<IActionResult> UpdateMargin([FromBody] UpdateMarginRequest request)
    {
        if (!User.IsInRole("CommercialDirector") && !User.IsInRole("GlobalAdmin"))
            return Forbid("Только руководитель может изменять маржинальность");

        var resource = await _db.ProjectResources
            .FirstOrDefaultAsync(r => r.Id == request.ResourceId && r.ProjectId == request.ProjectId);

        if (resource == null)
            return NotFound("Ресурс не найден");

        resource.MarginPercent = request.MarginPercent;

        var project = await _db.Projects
            .Include(p => p.Resources)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId);

        if (project != null)
            await _calc.RecalculateAsync(project);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            newFinalCost = resource.FinalCost,
            projectTotal = project?.TotalCostWithMargin ?? 0m,
            projectNetProfit = project?.NetProfit ?? 0m
        });
    }

    [HttpGet("{projectId}/summary")]
    public async Task<IActionResult> GetProjectSummary(int projectId)
    {
        var project = await _db.Projects
            .Include(p => p.Resources)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null) return NotFound();

        return Ok(new
        {
            projectId = project.Id,
            title = project.Title,
            totalCostWithoutMargin = project.TotalCostWithoutMargin,
            totalCostWithMargin = project.TotalCostWithMargin,
            netProfit = project.NetProfit,
            taxRate = project.TaxRate,
            resourceCount = project.Resources?.Count ?? 0,
            status = project.Status.ToString()
        });
    }
}

