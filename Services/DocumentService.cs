using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using WebWorkNew.Data;
using WebWorkNew.Models;

namespace WebWorkNew.Services;

public interface IDocumentService
{
    byte[] GenerateCommercialOfferPdf(Project project);
    byte[] GenerateNmaPdf(Project project);

    byte[] GenerateCommercialOfferWord(Project project);
    byte[] GenerateNmaWord(Project project);
}

public class DocumentService : IDocumentService
{
    private readonly AppDbContext _db;

    public DocumentService(AppDbContext db)
    {
        _db = db;
    }

    private CompanySettings? GetCompanySettings()
    {
        // В проекте предполагаем, что настройки компании одна на систему.
        return _db.CompanySettings.FirstOrDefault();
    }

    private WebWorkNew.Models.TechnicalTask? GetTechnicalTask(int projectId)
    {
        return _db.TechnicalTasks.FirstOrDefault(t => t.ProjectId == projectId);
    }

    public byte[] GenerateNmaWord(Project project)
    {
        var company = GetCompanySettings();
        if (project == null) return Array.Empty<byte>();

        using var ms = new MemoryStream();
        using var doc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Create(
            ms,
            DocumentFormat.OpenXml.WordprocessingDocumentType.Document,
            true);

        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(new DocumentFormat.OpenXml.Wordprocessing.Body());
        var body = mainPart.Document.Body;

        body.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text("РАСЧЕТ СТОИМОСТИ НЕМАТЕРИАЛЬНОГО АКТИВА (НМА)"))));
        body.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"Название проекта: {project.Title}"))));
        body.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"Срок исполнения/разработки: {project.StartDate:dd.MM.yyyy} - {project.EndDate:dd.MM.yyyy}"))));

        body.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"\nСтоимость НМА (итог без маржинальности): {project.TotalCostWithoutMargin:N2} ₽"))));

        body.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text("\nТаблица ресурсов (только себестоимость)"))));

        // Таблица ресурсов
        body.Append(GenerateNmaResourcesTable(project));

        // Подпись
        body.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text("Подпись"))));
        body.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"{company?.DirectorPosition ?? "Руководитель"}"))));
        body.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"{company?.DirectorFullName ?? ""}"))));

        mainPart.Document.Save();
        return ms.ToArray();
    }

    private static DocumentFormat.OpenXml.Wordprocessing.Table GenerateNmaResourcesTable(Project project)
    {
        var table = new DocumentFormat.OpenXml.Wordprocessing.Table();

        table.AppendChild(GenerateTableRow(new[] { "Ресурс", "Интервал", "Количество", "Себестоимость" }));

        foreach (var r in project.Resources)
        {
            var interval = $"{r.StartDate:dd.MM.yyyy} - {r.EndDate:dd.MM.yyyy}";
            var units = r.UnitsCount.ToString();
            var cost = r.CostPrice.ToString("N2");

            table.AppendChild(GenerateTableRow(new[] { r.ResourceName ?? "", interval, units, cost }));
        }

        return table;
    }

    private static DocumentFormat.OpenXml.Wordprocessing.Table GenerateServicesTable(Project project)
    {
        // Мини-таблица без сложного форматирования: чтобы Word открывался и содержимое было в правильных колонках.
        var table = new DocumentFormat.OpenXml.Wordprocessing.Table();

        // Header
        table.AppendChild(GenerateTableRow(new[] { "Название", "Количество (ед)", "Интервал оказания", "Стоимость" }));

        foreach (var r in project.Resources)
        {
            var interval = $"{r.StartDate:dd.MM.yyyy} - {r.EndDate:dd.MM.yyyy}";
            var units = r.UnitsCount.ToString();
            var serviceName = r.ResourceName;
            var cost = r.FinalCost.ToString("N2");

            table.AppendChild(GenerateTableRow(new[] { serviceName, units, interval, cost }));
        }

        return table;
    }

    private static DocumentFormat.OpenXml.Wordprocessing.TableRow GenerateTableRow(string[] cells)
    {
        var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

        foreach (var cell in cells)
        {
            var tc = new DocumentFormat.OpenXml.Wordprocessing.TableCell();
            tc.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                new DocumentFormat.OpenXml.Wordprocessing.Run(
                    new DocumentFormat.OpenXml.Wordprocessing.Text(cell ?? string.Empty))));

            row.Append(tc);
        }

        return row;
    }

    public byte[] GenerateCommercialOfferWord(Project project)
    {
        var company = GetCompanySettings();
        if (project == null) return Array.Empty<byte>();

        // ТЗ: таблица услуг КП: Название, Кол-во, Интервал оказания, Стоимость
        // Здесь делаем Word-документ с простой таблицей в 4 колонках.
        using var ms = new MemoryStream();
        using var doc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Create(
            ms,
            DocumentFormat.OpenXml.WordprocessingDocumentType.Document,
            true);

        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(new DocumentFormat.OpenXml.Wordprocessing.Body());
        var body = mainPart.Document.Body;

        body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
            new DocumentFormat.OpenXml.Wordprocessing.Run(
                new DocumentFormat.OpenXml.Wordprocessing.Text("КОММЕРЧЕСКОЕ ПРЕДЛОЖЕНИЕ"))));

        body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
            new DocumentFormat.OpenXml.Wordprocessing.Run(
                new DocumentFormat.OpenXml.Wordprocessing.Text($"Проект: {project.Title}"))));

        body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
            new DocumentFormat.OpenXml.Wordprocessing.Run(
                new DocumentFormat.OpenXml.Wordprocessing.Text($"Заказчик: {project.Customer?.FullName ?? "Не указан"}"))));

        body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
            new DocumentFormat.OpenXml.Wordprocessing.Run(
                new DocumentFormat.OpenXml.Wordprocessing.Text($"Дата формирования: {DateTime.Now:dd.MM.yyyy}"))));

        // Таблица услуг
        body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
            new DocumentFormat.OpenXml.Wordprocessing.Run(
                new DocumentFormat.OpenXml.Wordprocessing.Text("\nТаблица услуг"))));

        body.AppendChild(GenerateCommercialOfferServicesTableWord(project));

        body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
            new DocumentFormat.OpenXml.Wordprocessing.Run(
                new DocumentFormat.OpenXml.Wordprocessing.Text($"\nИтоговая стоимость: {project.TotalCostWithMargin:N2} ₽"))));

        // Подпись: должность и ФИО руководителя компании (из настроек системы)
        body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
            new DocumentFormat.OpenXml.Wordprocessing.Run(
                new DocumentFormat.OpenXml.Wordprocessing.Text("\nПодпись"))));

        body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
            new DocumentFormat.OpenXml.Wordprocessing.Run(
                new DocumentFormat.OpenXml.Wordprocessing.Text($"{company?.DirectorPosition ?? "Руководитель"}"))));

        body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
            new DocumentFormat.OpenXml.Wordprocessing.Run(
                new DocumentFormat.OpenXml.Wordprocessing.Text($"{company?.DirectorFullName ?? ""}"))));

        mainPart.Document.Save();
        return ms.ToArray();
    }

    private static DocumentFormat.OpenXml.Wordprocessing.Table GenerateCommercialOfferServicesTableWord(Project project)
    {
        var table = new DocumentFormat.OpenXml.Wordprocessing.Table();

        // Header строго по ТЗ
        table.AppendChild(GenerateTableRow(new[] { "Название", "Кол-во", "Интервал оказания", "Стоимость" }));

        foreach (var r in project.Resources)
        {
            var interval = $"{r.StartDate:dd.MM.yyyy} - {r.EndDate:dd.MM.yyyy}";
            var units = r.UnitsCount.ToString();
            var name = r.ServiceName ?? r.ResourceName ?? "Услуга";
            var cost = r.FinalCost.ToString("N2");

            table.AppendChild(GenerateTableRow(new[] { name, units, interval, cost }));
        }

        return table;
    }

    public byte[] GenerateCommercialOfferPdf(Project project)
    {
        var company = GetCompanySettings();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);

                page.Header()
                    .AlignCenter()
                    .Text("КОММЕРЧЕСКОЕ ПРЕДЛОЖЕНИЕ")
                    .FontSize(20)
                    .Bold()
                    .Underline();

                page.Content()
                    .PaddingVertical(20)
                    .Column(col =>
                    {
                        col.Item().Text($"Заказчик: {project.Customer?.FullName ?? "Не указан"}").FontSize(12);
                        col.Item().Text($"Компания: {project.Customer?.Name ?? "-"}").FontSize(12);
                        col.Item().Text($"Email: {project.Customer?.Email ?? "-"}").FontSize(12);
                        col.Item().Text($"Телефон: {project.Customer?.Phone ?? "-"}").FontSize(12);

                        col.Item().PaddingTop(15).Text($"Проект: {project.Title}").FontSize(14).Bold();
                        col.Item().Text($"Срок реализации: {project.StartDate:dd.MM.yyyy} - {project.EndDate:dd.MM.yyyy}");
                        col.Item().Text($"Описание: {project.Description ?? "-"}");

                        col.Item().PaddingTop(15).Text("Таблица услуг:")
                            .FontSize(12)
                            .Bold();

                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Название
                                columns.RelativeColumn(1); // Кол-во
                                columns.RelativeColumn(2); // Интервал оказания
                                columns.RelativeColumn(1.5f); // Стоимость
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Название").SemiBold().FontSize(10);
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Кол-во").SemiBold().FontSize(10);
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Интервал оказания").SemiBold().FontSize(10);
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Стоимость").SemiBold().FontSize(10);
                            });

                            foreach (var r in project.Resources)
                            {
                                table.Cell().Text(r.ServiceName ?? r.ResourceName ?? "Услуга").FontSize(9);
                                table.Cell().Text(r.UnitsCount.ToString()).FontSize(9);
                                table.Cell().Text($"{r.StartDate:dd.MM.yyyy} - {r.EndDate:dd.MM.yyyy}").FontSize(9);
                                table.Cell().AlignRight().Text($"{r.FinalCost:N2} ₽").FontSize(9);
                            }
                        });

                        col.Item().PaddingTop(18).Row(row =>
                        {
                            row.RelativeItem().Text("Итоговая стоимость (с маржинальностью):").Bold();
                            row.ConstantItem(170).AlignRight().Text($"{project.TotalCostWithMargin:N2} ₽").Bold();
                        });

                        col.Item().PaddingTop(28).Text($"Должность: {company?.DirectorPosition ?? "Руководитель"}").FontSize(10);
                        col.Item().Text($"ФИО: {company?.DirectorFullName ?? ""}").FontSize(10);
                        col.Item().PaddingTop(6).Text(DateTime.Now.ToString("dd.MM.yyyy")).FontSize(10);
                    });

                page.Footer()
                    .AlignCenter()
                    .Text("Система автоматического расчета стоимости IT-проектов")
                    .FontSize(8);
            });
        }).GeneratePdf();
    }

    public byte[] GenerateNmaPdf(Project project)
    {
        var company = GetCompanySettings();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);

                page.Header()
                    .AlignCenter()
                    .Text("РАСЧЕТ СТОИМОСТИ НЕМАТЕРИАЛЬНОГО АКТИВА (НМА)")
                    .FontSize(18)
                    .Bold();

                page.Content()
                    .PaddingVertical(20)
                    .Column(col =>
                    {
                        col.Item().Text($"Проект: {project.Title}").FontSize(14).Bold();
                        col.Item().Text($"Срок разработки: {project.StartDate:dd.MM.yyyy} - {project.EndDate:dd.MM.yyyy}");

                        col.Item().PaddingTop(15).Text("Таблица ресурсов (без маржинальности):")
                            .FontSize(12)
                            .Bold();

                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); // Ресурс
                                columns.RelativeColumn(1); // Начало
                                columns.RelativeColumn(1); // Конец
                                columns.RelativeColumn(0.6f); // Units
                                columns.RelativeColumn(1); // Себестоимость
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Ресурс").SemiBold().FontSize(8);
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Начало").SemiBold().FontSize(8);
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Конец").SemiBold().FontSize(8);
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Ед.").SemiBold().FontSize(8);
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Себестоимость").SemiBold().FontSize(8);
                            });

                            foreach (var r in project.Resources)
                            {
                                table.Cell().Text(r.ResourceName ?? "").FontSize(8);
                                table.Cell().Text(r.StartDate.ToString("dd.MM.yyyy")).FontSize(8);
                                table.Cell().Text(r.EndDate.ToString("dd.MM.yyyy")).FontSize(8);
                                table.Cell().Text(r.UnitsCount.ToString()).FontSize(8);
                                table.Cell().AlignRight().Text($"{r.CostPrice:N2} ₽").FontSize(8);
                            }
                        });

                        col.Item().PaddingTop(18).Row(row =>
                        {
                            row.RelativeItem().Text("Стоимость НМА (себестоимость):").Bold();
                            row.ConstantItem(170).AlignRight().Text($"{project.TotalCostWithoutMargin:N2} ₽").Bold();
                        });

                        col.Item().PaddingTop(22).Text("Данный документ является основанием для постановки на баланс").FontSize(10);
                        col.Item().PaddingTop(6).Text($"Должность: {company?.DirectorPosition ?? "Руководитель"}").FontSize(10);
                        col.Item().Text($"ФИО: {company?.DirectorFullName ?? ""}").FontSize(10);
                        col.Item().PaddingTop(6).Text(DateTime.Now.ToString("dd.MM.yyyy")).FontSize(10);
                    });

                page.Footer()
                    .AlignCenter()
                    .Text("Система расчета стоимости IT-проектов")
                    .FontSize(8);
            });
        }).GeneratePdf();
    }
}

