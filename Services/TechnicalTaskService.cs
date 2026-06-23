using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using System.Text;
using WebWorkNew.Data;
using WebWorkNew.Models;

namespace WebWorkNew.Services;

public class TechnicalTaskService : ITechnicalTaskService
{
    private readonly AppDbContext _db;

    public TechnicalTaskService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<TechnicalTask?> GetByProjectIdAsync(int projectId)
    {
        return await _db.TechnicalTasks
            .FirstOrDefaultAsync(tt => tt.ProjectId == projectId);
    }

    public async Task<TechnicalTask> CreateOrUpdateAsync(TechnicalTask technicalTask)
    {
        var existing = await _db.TechnicalTasks
            .FirstOrDefaultAsync(tt => tt.ProjectId == technicalTask.ProjectId);

        technicalTask.UpdatedAt = DateTime.Now;

        if (existing == null)
        {
            technicalTask.CreatedAt = DateTime.Now;
            _db.TechnicalTasks.Add(technicalTask);
        }
        else
        {
            existing.Title = technicalTask.Title;
            existing.Version = technicalTask.Version;
            existing.Goals = technicalTask.Goals;
            existing.FunctionalRequirements = technicalTask.FunctionalRequirements;
            existing.NonFunctionalRequirements = technicalTask.NonFunctionalRequirements;
            existing.SystemComposition = technicalTask.SystemComposition;
            existing.TechStack = technicalTask.TechStack;
            existing.UiRequirements = technicalTask.UiRequirements;
            existing.Documentation = technicalTask.Documentation;
            existing.Notes = technicalTask.Notes;
            existing.Status = technicalTask.Status;
            existing.UpdatedAt = technicalTask.UpdatedAt;
            
            _db.TechnicalTasks.Update(existing);
            technicalTask = existing;
        }

        await _db.SaveChangesAsync();
        return technicalTask;
    }

    public async Task<string> GenerateHtmlAsync(TechnicalTask technicalTask)
    {
        var project = await _db.Projects.FindAsync(technicalTask.ProjectId);
        
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset='utf-8'>");
        sb.AppendLine($"<title>{EscapeHtml(technicalTask.Title)}</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; margin: 40px; line-height: 1.6; color: #333; }");
        sb.AppendLine("h1 { color: #2a5298; border-bottom: 3px solid #2a5298; padding-bottom: 10px; }");
        sb.AppendLine("h2 { color: #1e3c72; margin-top: 30px; border-left: 4px solid #2a5298; padding-left: 15px; }");
        sb.AppendLine(".header { text-align: center; margin-bottom: 30px; }");
        sb.AppendLine(".meta { background: #f5f5f5; padding: 15px; border-radius: 8px; margin-bottom: 20px; border-left: 4px solid #2a5298; }");
        sb.AppendLine(".section { margin-bottom: 25px; }");
        sb.AppendLine(".section-title { background: #e8f0fe; padding: 10px 15px; border-radius: 5px; font-weight: bold; font-size: 16px; }");
        sb.AppendLine(".content { padding: 15px; background: #fafafa; border-radius: 5px; white-space: pre-wrap; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        
        sb.AppendLine("<div class='header'>");
        sb.AppendLine($"<h1>{EscapeHtml(technicalTask.Title)}</h1>");
        sb.AppendLine($"<p><strong>Версия:</strong> {EscapeHtml(technicalTask.Version)} | <strong>Статус:</strong> {GetStatusText(technicalTask.Status)}</p>");
        sb.AppendLine("</div>");
        
        sb.AppendLine("<div class='meta'>");
        sb.AppendLine($"<p><strong>Проект:</strong> {EscapeHtml(project?.Title ?? "Не указан")}</p>");
        sb.AppendLine($"<p><strong>Дата создания:</strong> {technicalTask.CreatedAt:dd.MM.yyyy}</p>");
        sb.AppendLine($"<p><strong>Дата обновления:</strong> {technicalTask.UpdatedAt:dd.MM.yyyy}</p>");
        sb.AppendLine("</div>");
        
        AddSection(sb, "Цели и задачи", technicalTask.Goals);
        AddSection(sb, "Функциональные требования", technicalTask.FunctionalRequirements);
        AddSection(sb, "Нефункциональные требования", technicalTask.NonFunctionalRequirements);
        AddSection(sb, "Состав системы", technicalTask.SystemComposition);
        AddSection(sb, "Технологический стек", technicalTask.TechStack);
        AddSection(sb, "Требования к интерфейсу", technicalTask.UiRequirements);
        AddSection(sb, "Состав документации", technicalTask.Documentation);
        AddSection(sb, "Примечания", technicalTask.Notes);
        
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        
        return sb.ToString();
    }

    public async Task<byte[]> GeneratePdfAsync(TechnicalTask technicalTask)
    {
        // ТЗ: требуется PDF.
        // В текущем проекте нет полноценного HTML->PDF движка, поэтому создаём простой PDF через QuestPDF,
        // используя те же данные ТЗ.
        var project = await _db.Projects.FindAsync(technicalTask.ProjectId);

        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);

                page.Header().AlignCenter().Text("ТЕХНИЧЕСКОЕ ЗАДАНИЕ").FontSize(18).Bold();

                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Item().Text($"Проект: {project?.Title ?? "Не указан"}").FontSize(12);
                    col.Item().Text($"Версия: {technicalTask.Version}").FontSize(12);
                    col.Item().Text($"Статус: {GetStatusText(technicalTask.Status)}").FontSize(12);
                    col.Item().Text($"Создано: {technicalTask.CreatedAt:dd.MM.yyyy}").FontSize(12);
                    col.Item().Text($"Обновлено: {technicalTask.UpdatedAt:dd.MM.yyyy}").FontSize(12);

                    col.Item().PaddingTop(10).Text(technicalTask.Title).FontSize(14).Bold();

                    void AddBlock(string title, string? content)
                    {
                        if (string.IsNullOrWhiteSpace(content)) return;
                        col.Item().PaddingTop(10).Text(title).FontSize(12).Bold();
                        foreach (var line in content.Replace("\r\n", "\n").Split('\n'))
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;
                            col.Item().Text(line).FontSize(10);
                        }
                    }

                    AddBlock("Цели и задачи", technicalTask.Goals);
                    AddBlock("Функциональные требования", technicalTask.FunctionalRequirements);
                    AddBlock("Нефункциональные требования", technicalTask.NonFunctionalRequirements);
                    AddBlock("Состав системы", technicalTask.SystemComposition);
                    AddBlock("Технологический стек", technicalTask.TechStack);
                    AddBlock("Требования к интерфейсу", technicalTask.UiRequirements);
                    AddBlock("Состав документации", technicalTask.Documentation);
                    AddBlock("Примечания", technicalTask.Notes);
                });

                page.Footer().AlignCenter().Text("Система автоматического расчёта стоимости IT-проектов").FontSize(8);
            });
        }).GeneratePdf();
    }


    public async Task<byte[]> GenerateWordAsync(TechnicalTask technicalTask)
    {
        return await GenerateDocxAsync(technicalTask);
    }

    private async Task<byte[]> GenerateDocxAsync(TechnicalTask technicalTask)
    {
        var project = await _db.Projects.FindAsync(technicalTask.ProjectId);

        using var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, DocumentFormat.OpenXml.WordprocessingDocumentType.Document, true))
        {
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(new Body());

            var body = mainPart.Document.Body;

            // Заголовок
            body.AppendChild(new Paragraph(
                new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
                new Run(new Text("Техническое задание") { Space = SpaceProcessingModeValues.Preserve })));

            body.AppendChild(new Paragraph(new Run(new Text(technicalTask.Title ?? string.Empty) { Space = SpaceProcessingModeValues.Preserve })));

            body.AppendChild(AppendBlankLine());

            // Метаданные
            AppendKeyValue(body, "Версия", technicalTask.Version?.ToString());
            AppendKeyValue(body, "Статус", GetStatusText(technicalTask.Status));
            AppendKeyValue(body, "Проект", project?.Title);
            AppendKeyValue(body, "Дата создания", technicalTask.CreatedAt.ToString("dd.MM.yyyy"));
            AppendKeyValue(body, "Дата обновления", technicalTask.UpdatedAt.ToString("dd.MM.yyyy"));

            body.AppendChild(AppendBlankLine());

            AddSection(body, "Цели и задачи", technicalTask.Goals);
            AddSection(body, "Функциональные требования", technicalTask.FunctionalRequirements);
            AddSection(body, "Нефункциональные требования", technicalTask.NonFunctionalRequirements);
            AddSection(body, "Состав системы", technicalTask.SystemComposition);
            AddSection(body, "Технологический стек", technicalTask.TechStack);
            AddSection(body, "Требования к интерфейсу", technicalTask.UiRequirements);
            AddSection(body, "Состав документации", technicalTask.Documentation);
            AddSection(body, "Примечания", technicalTask.Notes);

            doc.MainDocumentPart?.Document.Save();
        }

        return ms.ToArray();
    }

    private static Paragraph AppendBlankLine() => new Paragraph(new Run(new Text("")));

    private static void AppendKeyValue(Body body, string key, string? value)
    {
        var safe = value ?? string.Empty;
        body.AppendChild(new Paragraph(
            new Run(new Text($"{key}: {safe}"))));
    }

    private static void AddSection(Body body, string title, string? content)
    {
        if (string.IsNullOrWhiteSpace(content)) return;

        body.AppendChild(new Paragraph(
            new Run(new Text(title) { Space = SpaceProcessingModeValues.Preserve } )));

        foreach (var line in content.Replace("\r\n", "\n").Split('\n'))
        {
            if (line == null) continue;
            body.AppendChild(new Paragraph(new Run(new Text(line) { Space = SpaceProcessingModeValues.Preserve })));
        }

        body.AppendChild(AppendBlankLine());
    }

    public async Task<bool> ValidateAsync(TechnicalTask technicalTask)

    {
        if (string.IsNullOrWhiteSpace(technicalTask.Title))
            return false;
            
        if (technicalTask.ProjectId <= 0)
            return false;
            
        var project = await _db.Projects.FindAsync(technicalTask.ProjectId);
        if (project == null)
            return false;
            
        return true;
    }

    private void AddSection(StringBuilder sb, string title, string? content)
    {
        if (string.IsNullOrWhiteSpace(content)) return;
        
        sb.AppendLine("<div class='section'>");
        sb.AppendLine($"<div class='section-title'>{EscapeHtml(title)}</div>");
        sb.AppendLine($"<div class='content'>{EscapeHtml(content).Replace("\n", "<br/>")}</div>");
        sb.AppendLine("</div>");
    }

    private string EscapeHtml(string text) => 
        string.IsNullOrEmpty(text) ? "" : 
        text.Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    
    private string GetStatusText(TechnicalTaskStatus status) => status switch
    {
        TechnicalTaskStatus.Draft => "Черновик",
        TechnicalTaskStatus.UnderReview => "На согласовании",
        TechnicalTaskStatus.Approved => "Согласовано",
        TechnicalTaskStatus.InProgress => "В работе",
        TechnicalTaskStatus.Completed => "Завершено",
        _ => "Неизвестно"
    };
}