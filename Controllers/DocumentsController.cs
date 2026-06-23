using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebWorkNew.Data;
using WebWorkNew.Services;

namespace WebWorkNew.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IDocumentService _docs;
    private readonly IExcelExportService _excel;

    public DocumentsController(AppDbContext db, IDocumentService docs, IExcelExportService excel)
    {
        _db = db;
        _docs = docs;
        _excel = excel;
    }

    [HttpGet("project/{id}/commercial-offer/pdf")]
    public async Task<IActionResult> CommercialOfferPdf(int id)
    {
        var project = await _db.Projects
            .Include(p => p.Resources)
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        return NotFound();

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

    [HttpGet("project/{id}/commercial-offer/excel")]
    public async Task<IActionResult> CommercialOfferExcel(int id)
    {
        var project = await _db.Projects
            .Include(p => p.Resources)
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null) return NotFound();

        var excel = _excel.ExportCommercialOfferToExcel(project);
        return File(excel,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Коммерческое_предложение_проект_{id}.xlsx");
    }

    [HttpGet("project/{id}/nma/excel")]
    public async Task<IActionResult> NmaExcel(int id)
    {
        var project = await _db.Projects
            .Include(p => p.Resources)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null) return NotFound();

        var excel = _excel.ExportNmaToExcel(project);
        return File(excel,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"НМА_проект_{id}.xlsx");
    }

    [HttpGet("project/{id}/commercial-offer/word")]
    public async Task<IActionResult> CommercialOfferWord(int id)
    {
        var project = await _db.Projects
            .Include(p => p.Resources)
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null) 
        return NotFound();

        var company = await _db.CompanySettings.FirstOrDefaultAsync();
        if (company == null) 
        return NotFound();

        var bytes = _docs.GenerateCommercialOfferWord(project);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            $"Коммерческое_предложение_проект_{id}.docx");


    }

    [HttpGet("project/{id}/nma/word")]
    public async Task<IActionResult> NmaWord(int id)
    {
        var project = await _db.Projects
            .Include(p => p.Resources)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null) 
        return NotFound();

        var bytes = _docs.GenerateNmaWord(project);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            $"НМА_проект_{id}.docx");

    }
}

// WordDocFactory больше не используется (Word генерируется в DocumentService)

internal static class WordDocFactory
{
    public static byte[] GenerateCommercialOfferDocx(Models.Project project)
    {
        using var ms = new MemoryStream();
        using var doc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Create(
            ms,
            DocumentFormat.OpenXml.WordprocessingDocumentType.Document,
            true);

        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(new DocumentFormat.OpenXml.Wordprocessing.Body());
        var body = mainPart.Document.Body;

        body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
            new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text("КОММЕРЧЕСКОЕ ПРЕДЛОЖЕНИЕ"))));

        body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
            new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"Проект: {project.Title}"))));

        body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
            new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"Заказчик: {project.Customer?.FullName ?? "Не указан"}"))));

        body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
            new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"Итог: {project.TotalCostWithMargin:N2} ₽"))));

        mainPart.Document.Save();
        return ms.ToArray();
    }

    public static byte[] GenerateNmaDocx(Models.Project project)
    {
        using var ms = new MemoryStream();
        using var doc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Create(
            ms,
            DocumentFormat.OpenXml.WordprocessingDocumentType.Document,
            true);

        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(new DocumentFormat.OpenXml.Wordprocessing.Body());
        var body = mainPart.Document.Body;

        body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
            new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text("РАСЧЕТ СТОИМОСТИ НМА"))));

        body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
            new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"Проект: {project.Title}"))));

        body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
            new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"Себестоимость: {project.TotalCostWithoutMargin:N2} ₽"))));

        mainPart.Document.Save();
        return ms.ToArray();
    }
}
