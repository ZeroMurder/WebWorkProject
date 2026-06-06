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

    [HttpGet("project/{id}/commercial-offer/pdf")]
    public async Task<IActionResult> CommercialOfferPdf(int id)
    {
        var project = await _db.Projects
            .Include(p => p.Resources)
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null) return NotFound();

        var pdf = _docs.GenerateCommercialOfferPdf(project);
        return File(pdf, "application/pdf", $"Коммерческое_предложение_проект_{id}.pdf");
    }

    [HttpGet("project/{id}/nma/pdf")]
    public async Task<IActionResult> NmaPdf(int id)
    {
        var project = await _db.Projects
            .Include(p => p.Resources)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null) return NotFound();

        var pdf = _docs.GenerateNmaPdf(project);
        return File(pdf, "application/pdf", $"НМА_проект_{id}.pdf");
    }
}