using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebWork.Data;
using WebWork.Services;

namespace WebWork.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IDocumentService _docs;

    public DocumentsController(AppDbContext db, IDocumentService docs)
    {
        _db = db;
        _docs = docs;
    }

    [HttpGet("project/{id}/pdf")]
    public async Task<IActionResult> ProjectPdf(int id)
    {
        var project = await _db.Projects
            .Include(x => x.Resources)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (project == null) return NotFound();

        var pdf = _docs.GenerateProjectPdf(project);
        return File(pdf, "application/pdf", $"project-{id}.pdf");
    }
}