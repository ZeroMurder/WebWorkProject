using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using WebWork.Models;

namespace WebWork.Services;

public interface IDocumentService
{
    byte[] GenerateCommercialOfferPdf(Project project);
    byte[] GenerateNmaPdf(Project project);
}

public class DocumentService : IDocumentService
{
    private static CompanySettings? GetCompanySettings(Project project)
    {
        // В WebWorkNew сейчас DocumentService не получает AppDbContext, поэтому CompanySettings в документ подтянуть невозможно.
        // Возвращаем null — в таком случае будут использованы дефолтные подписи.
        return null;
    }

    public byte[] GenerateCommercialOfferPdf(Project project)
    {
        var company = GetCompanySettings(project);

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
                        // Реквизиты заказчика
                        col.Item().Text($"Заказчик: {project.Customer?.FullName ?? "Не указан"}").FontSize(12);
                        col.Item().Text($"Компания: {project.Customer?.Name ?? "-"}").FontSize(12);
                        col.Item().Text($"Email: {project.Customer?.Email ?? "-"}").FontSize(12);
                        col.Item().Text($"Телефон: {project.Customer?.Phone ?? "-"}").FontSize(12);

                        col.Item().PaddingTop(15).Text($"Проект: {project.Title}").FontSize(14).Bold();
                        col.Item().Text($"Срок реализации: {project.StartDate:dd.MM.yyyy} - {project.EndDate:dd.MM.yyyy}");
                        col.Item().Text($"Описание: {project.Description ?? "-"}");

                        col.Item().PaddingTop(15).Text("Таблица услуг (интервал/даты + стоимость с маржинальностью):")
                            .FontSize(12)
                            .Bold();

                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(140); // Ресурс
                                columns.ConstantColumn(100); // Услуга
                                columns.ConstantColumn(100); // Начало
                                columns.ConstantColumn(100); // Конец
                                columns.ConstantColumn(60); // Units
                                columns.ConstantColumn(120); // Стоимость
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Ресурс").SemiBold().FontSize(9);
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Услуга").SemiBold().FontSize(9);
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Начало").SemiBold().FontSize(9);
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Конец").SemiBold().FontSize(9);
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Ед.").SemiBold().FontSize(9);
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Стоимость").SemiBold().FontSize(9);
                            });

                            foreach (var r in project.Resources)
                            {
                                table.Cell().Text(r.ResourceName).FontSize(9);
                                table.Cell().Text(r.ServiceName ?? r.Type.ToString()).FontSize(9);
                                table.Cell().Text(r.StartDate.ToString("dd.MM.yyyy")).FontSize(9);
                                table.Cell().Text(r.EndDate.ToString("dd.MM.yyyy")).FontSize(9);
                                table.Cell().Text(r.UnitsCount.ToString()).FontSize(9);
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
        var company = GetCompanySettings(project);

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
                                columns.ConstantColumn(180); // Ресурс
                                columns.ConstantColumn(120); // Начало
                                columns.ConstantColumn(120); // Конец
                                columns.ConstantColumn(60); // Units
                                columns.ConstantColumn(120); // Себестоимость
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Ресурс").SemiBold().FontSize(9);
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Начало").SemiBold().FontSize(9);
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Конец").SemiBold().FontSize(9);
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Ед.").SemiBold().FontSize(9);
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Себестоимость").SemiBold().FontSize(9);
                            });

                            foreach (var r in project.Resources)
                            {
                                table.Cell().Text(r.ResourceName).FontSize(9);
                                table.Cell().Text(r.StartDate.ToString("dd.MM.yyyy")).FontSize(9);
                                table.Cell().Text(r.EndDate.ToString("dd.MM.yyyy")).FontSize(9);
                                table.Cell().Text(r.UnitsCount.ToString()).FontSize(9);
                                table.Cell().AlignRight().Text($"{r.CostPrice:N2} ₽").FontSize(9);
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

