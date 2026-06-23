using ClosedXML.Excel;
using System.Drawing;
using WebWorkNew.Models;

namespace WebWorkNew.Services;

public class ExcelExportService : IExcelExportService
{
    public ExcelExportService()
    {
        // ClosedXML не требует явной установки лицензии в большинстве сценариев.
    }

    public byte[] ExportProjectsToExcel(List<Project> projects)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Проекты");

        ws.Cell(1, 1).Value = "ID";
        ws.Cell(1, 2).Value = "Название проекта";
        ws.Cell(1, 3).Value = "Начало";
        ws.Cell(1, 4).Value = "Окончание";
        ws.Cell(1, 5).Value = "Заказчик";
        ws.Cell(1, 6).Value = "Себестоимость";
        ws.Cell(1, 7).Value = "Стоимость с маржой";
        ws.Cell(1, 8).Value = "Чистая прибыль";

        var header = ws.Range(1, 1, 1, 8);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;


        for (int i = 0; i < projects.Count; i++)
        {
            var p = projects[i];
            int row = i + 2;

            ws.Cell(row, 1).Value = p.Id;
            ws.Cell(row, 2).Value = p.Title;
            ws.Cell(row, 3).Value = p.StartDate;
            ws.Cell(row, 4).Value = p.EndDate;
            ws.Cell(row, 5).Value = p.Customer?.Name ?? p.Customer?.FullName ?? "-";
            ws.Cell(row, 6).Value = p.TotalCostWithoutMargin;
            ws.Cell(row, 7).Value = p.TotalCostWithMargin;
            ws.Cell(row, 8).Value = p.NetProfit;

            ws.Cell(row, 3).Style.DateFormat.Format = "dd.MM.yyyy";
            ws.Cell(row, 4).Style.DateFormat.Format = "dd.MM.yyyy";
        }

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] ExportResourcesToExcel(List<ProjectResource> resources)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Ресурсы");

        ws.Cell(1, 1).Value = "Название";
        ws.Cell(1, 2).Value = "Тип";
        ws.Cell(1, 3).Value = "Услуга";
        ws.Cell(1, 4).Value = "Начало";
        ws.Cell(1, 5).Value = "Конец";
        ws.Cell(1, 6).Value = "Кол-во";
        ws.Cell(1, 7).Value = "Себестоимость";
        ws.Cell(1, 8).Value = "Маржа %";
        ws.Cell(1, 9).Value = "Итоговая стоимость";

        var header = ws.Range(1, 1, 1, 9);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;


        for (int i = 0; i < resources.Count; i++)
        {
            var r = resources[i];
            int row = i + 2;

            ws.Cell(row, 1).Value = r.ResourceName;
            ws.Cell(row, 2).Value = r.Type.ToString();
            ws.Cell(row, 3).Value = r.ServiceName;
            ws.Cell(row, 4).Value = r.StartDate;
            ws.Cell(row, 5).Value = r.EndDate;
            ws.Cell(row, 6).Value = r.UnitsCount;
            ws.Cell(row, 7).Value = r.CostPrice;
            ws.Cell(row, 8).Value = r.MarginPercent;
            ws.Cell(row, 9).Value = r.FinalCost;

            ws.Cell(row, 4).Style.DateFormat.Format = "dd.MM.yyyy";
            ws.Cell(row, 5).Style.DateFormat.Format = "dd.MM.yyyy";
        }

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

public byte[] ExportCommercialOfferToExcel(Project project)
{
    using var wb = new XLWorkbook();
    var ws = wb.Worksheets.Add("Коммерческое_предложение");

    // Заголовок
    ws.Cell(1, 1).Value = "КОММЕРЧЕСКОЕ ПРЕДЛОЖЕНИЕ";
    ws.Range(1, 1, 1, 4).Merge();
    ws.Cell(1, 1).Style.Font.Bold = true;
    ws.Cell(1, 1).Style.Font.FontSize = 16;

    int row = 3;
    
    // Реквизиты заказчика
    ws.Cell(row, 1).Value = "Заказчик:";
    ws.Cell(row, 2).Value = project.Customer?.FullName ?? "-";
    row++;
    ws.Cell(row, 1).Value = "Компания:";
    ws.Cell(row, 2).Value = project.Customer?.Name ?? "-";
    row++;
    ws.Cell(row, 1).Value = "Email:";
    ws.Cell(row, 2).Value = project.Customer?.Email ?? "-";
    row++;
    ws.Cell(row, 1).Value = "Телефон:";
    ws.Cell(row, 2).Value = project.Customer?.Phone ?? "-";
    row += 2;

    // Информация о проекте
    ws.Cell(row, 1).Value = $"Проект: {project.Title}";
    ws.Range(row, 1, row, 4).Merge();
    ws.Cell(row, 1).Style.Font.Bold = true;
    row++;
    ws.Cell(row, 1).Value = $"Срок: {project.StartDate:dd.MM.yyyy} - {project.EndDate:dd.MM.yyyy}";
    ws.Range(row, 1, row, 4).Merge();
    row += 2;

    // ТАБЛИЦА УСЛУГ - строго по ТЗ: Название, Кол-во, Интервал оказания, Стоимость
    ws.Cell(row, 1).Value = "Название";
    ws.Cell(row, 2).Value = "Кол-во";
    ws.Cell(row, 3).Value = "Интервал оказания";
    ws.Cell(row, 4).Value = "Стоимость";

    var header = ws.Range(row, 1, row, 4);
    header.Style.Font.Bold = true;
    header.Style.Fill.BackgroundColor = XLColor.LightGray;
    row++;

    // Заполняем данные
    foreach (var r in project.Resources)
    {
        ws.Cell(row, 1).Value = r.ServiceName ?? r.ResourceName ?? "Услуга";
        ws.Cell(row, 2).Value = r.UnitsCount;
        ws.Cell(row, 3).Value = $"{r.StartDate:dd.MM.yyyy} - {r.EndDate:dd.MM.yyyy}";
        ws.Cell(row, 4).Value = r.FinalCost;
        row++;
    }

    // ИТОГО
    row++;
    ws.Cell(row, 3).Value = "ИТОГО:";
    ws.Cell(row, 4).Value = project.TotalCostWithMargin;
    ws.Range(row, 3, row, 4).Style.Font.Bold = true;

    ws.Columns().AdjustToContents();

    using var ms = new MemoryStream();
    wb.SaveAs(ms);
    return ms.ToArray();
}

    public byte[] ExportNmaToExcel(Project project)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("НМА");

        ws.Cell(1, 1).Value = "РАСЧЕТ СТОИМОСТИ НЕМАТЕРИАЛЬНОГО АКТИВА";
        ws.Range(1, 1, 1, 3).Merge();
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        int row = 3;
        ws.Cell(row, 1).Value = $"Проект: {project.Title}";
        ws.Range(row, 1, row, 2).Merge();
        row++;
        ws.Cell(row, 1).Value = $"Срок: {project.StartDate:dd.MM.yyyy} - {project.EndDate:dd.MM.yyyy}";
        ws.Range(row, 1, row, 2).Merge();
        row += 2;

        ws.Cell(row, 1).Value = "Ресурс";
        ws.Cell(row, 2).Value = "Начало";
        ws.Cell(row, 3).Value = "Конец";
        ws.Cell(row, 4).Value = "Кол-во";
        ws.Cell(row, 5).Value = "Себестоимость";

        var header = ws.Range(row, 1, row, 5);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;


        row++;
        foreach (var r in project.Resources)
        {
            ws.Cell(row, 1).Value = r.ResourceName;
            ws.Cell(row, 2).Value = r.StartDate;
            ws.Cell(row, 3).Value = r.EndDate;
            ws.Cell(row, 4).Value = r.UnitsCount;
            ws.Cell(row, 5).Value = r.CostPrice;

            ws.Cell(row, 2).Style.DateFormat.Format = "dd.MM.yyyy";
            ws.Cell(row, 3).Style.DateFormat.Format = "dd.MM.yyyy";
            row++;
        }

        row++;
        ws.Cell(row, 4).Value = "ИТОГО:";
        ws.Cell(row, 5).Value = project.TotalCostWithoutMargin;
        ws.Range(row, 4, row, 5).Style.Font.Bold = true;

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] ExportWorkspaceUsersToExcel(Workspace workspace, List<WorkspaceUser> users)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Пользователи_рабочей_области");

        ws.Cell(1, 1).Value = $"Рабочая область: {workspace.Name}";
        ws.Range(1, 1, 1, 4).Merge();
        ws.Cell(1, 1).Style.Font.Bold = true;

        ws.Cell(3, 1).Value = "Пользователь";
        ws.Cell(3, 2).Value = "Email";
        ws.Cell(3, 3).Value = "Должность";
        ws.Cell(3, 4).Value = "Права";

        var header = ws.Range(3, 1, 3, 4);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;


        int row = 4;
        foreach (var wu in users)
        {
            var rights = new List<string>();
            if (wu.CanView) rights.Add("Просмотр");
            if (wu.CanEditProjects) rights.Add("Редактирование проектов");
            if (wu.CanManageWorkspace) rights.Add("Управление областью");

            ws.Cell(row, 1).Value = wu.User != null ? $"{wu.User.LastName} {wu.User.FirstName}" : wu.UserId;
            ws.Cell(row, 2).Value = wu.User?.Email;
            ws.Cell(row, 3).Value = wu.User?.Position;
            ws.Cell(row, 4).Value = string.Join(", ", rights);

            row++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}

