using Microsoft.EntityFrameworkCore;
using WebWorkNew.Data;
using WebWorkNew.Enums;
using WebWorkNew.Models;

namespace WebWorkNew.Services;

public class ProjectCalculationService : IProjectCalculationService
{
    private readonly AppDbContext _db;

    public ProjectCalculationService(AppDbContext db) => _db = db;

    public async Task RecalculateAsync(Project project)
    {
        if (project == null) return;

        // Reload resources if needed
        if (project.Resources == null || !project.Resources.Any())
        {
            var freshProject = await _db.Projects
                .Include(p => p.Resources)
                .FirstOrDefaultAsync(p => p.Id == project.Id);
            
            if (freshProject != null && freshProject.Resources != null)
            {
                project.Resources = freshProject.Resources;
            }
        }

        decimal totalBase = 0;
        decimal totalFinal = 0;
        decimal profit = 0;

        if (project.Resources != null)
        {
            foreach (var r in project.Resources)
            {
                if (r == null) continue;
                
                decimal cost = await CalculateResourceCost(r);
                
                r.CostPrice = cost;
                r.FinalCost = cost + cost * r.MarginPercent / 100m;

                totalBase += r.CostPrice;
                totalFinal += r.FinalCost;
                profit += r.FinalCost - r.CostPrice;
            }
        }

        project.TotalCostWithoutMargin = totalBase;
        project.TotalCostWithMargin = totalFinal + totalFinal * (project.TaxRate / 100m);
        project.NetProfit = profit;
    }

    private async Task<decimal> CalculateResourceCost(ProjectResource resource)
    {
        if (resource == null) return 0;
        
        switch (resource.Type)
        {
            case ResourceType.Employee when resource.EmployeeId.HasValue:
                return await CalculateEmployeeCostExactAsync(resource);

            case ResourceType.Executor when resource.ExecutorId.HasValue:
                var ex = await _db.Executors.FindAsync(resource.ExecutorId.Value);
                if (ex != null)
                {
                    // Для НПД налог не добавляется (налог платит самозанятый)
                    decimal taxRate;
                    if (Enum.IsDefined(typeof(EmploymentType), ex.EmploymentType) && 
                        ex.EmploymentType == EmploymentType.NPD)
                    {
                        taxRate = 0m;
                    }
                    else
                    {
                        taxRate = ex.TaxRate;
                    }
                    return ex.CalculateCost(resource.UnitsCount, taxRate);
                }
                break;

            case ResourceType.Subcontractor when resource.SubcontractorId.HasValue:
                var sub = await _db.Subcontractors.FindAsync(resource.SubcontractorId.Value);
                if (sub != null)
                {
                    // Субподрядчики всегда с налогом (ЮЛ/ИП)
                    return sub.CalculateCost(resource.UnitsCount);
                }
                break;

            case ResourceType.Equipment when resource.EquipmentId.HasValue:
                var eq = await _db.Equipments.FindAsync(resource.EquipmentId.Value);
                if (eq != null)
                    return eq.CalculateCost(resource.UnitsCount);
                break;
        }
        
        return 0;
    }

    /// <summary>
    /// Получение количества рабочих дней в периоде (Пн-Пт, исключая праздники)
    /// </summary>
    private int GetWorkingDaysInPeriod(DateTime start, DateTime end)
    {
        var days = 0;
        // Получаем праздники для данного периода
        var holidays = GetHolidays(start, end);
        
        for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
        {
            if (IsWorkingDay(date, holidays))
                days++;
        }
        return days > 0 ? days : 1;
    }

    /// <summary>
    /// Проверка, является ли день рабочим
    /// </summary>
    private bool IsWorkingDay(DateTime date, HashSet<DateTime> holidays)
    {
        return date.DayOfWeek != DayOfWeek.Saturday && 
               date.DayOfWeek != DayOfWeek.Sunday &&
               !holidays.Contains(date.Date);
    }

    /// <summary>
    /// Получение списка праздничных дней для периода
    /// </summary>
    private HashSet<DateTime> GetHolidays(DateTime start, DateTime end)
    {
        try
        {
            // Проверяем, существует ли таблица Holidays в БД
            // Если нет - возвращаем пустой HashSet
            var holidays = _db.Holidays
                .Where(h => h.Date >= start.Date && h.Date <= end.Date)
                .Select(h => h.Date.Date)
                .ToHashSet();
            return holidays;
        }
        catch
        {
            // Если таблица Holidays не существует, возвращаем пустой набор
            return new HashSet<DateTime>();
        }
    }

    private static IEnumerable<(DateTime from, DateTime to)> GetMonthlySegments(DateTime start, DateTime end)
    {
        var current = new DateTime(start.Year, start.Month, 1);
        var last = new DateTime(end.Year, end.Month, 1);

        while (current <= last)
        {
            var from = current < start ? start : current;
            var monthEnd = new DateTime(current.Year, current.Month, DateTime.DaysInMonth(current.Year, current.Month));
            var to = monthEnd > end ? end : monthEnd;

            yield return (from.Date, to.Date);
            current = current.AddMonths(1);
        }
    }

private async Task<decimal> CalculateEmployeeCostExactAsync(ProjectResource resource)
{
    if (resource.EmployeeId is null) return 0m;
    var emp = await _db.Employees.FindAsync(resource.EmployeeId.Value);
    if (emp is null) return 0m;

    var segments = GetMonthlySegments(resource.StartDate, resource.EndDate).ToList();
    if (segments.Count == 0) return 0m;

    decimal totalCost = 0m;

    foreach (var (from, to) in segments)
    {
        // Количество рабочих дней в этом месяце (весь месяц)
        var workingDaysInMonth = GetWorkingDaysInPeriod(
            new DateTime(from.Year, from.Month, 1),
            new DateTime(from.Year, from.Month, DateTime.DaysInMonth(from.Year, from.Month))
        );
        if (workingDaysInMonth <= 0) continue;

        // Количество рабочих дней из интервала проекта в этом месяце
        var daysInProject = GetWorkingDaysInPeriod(from, to);
        if (daysInProject <= 0) continue;

        // Расчёт с учетом налоговой ставки (в процентах)
        var monthlySalaryWithTax = emp.MonthlySalary + emp.MonthlySalary * (emp.TaxRate / 100m);
        var monthCostPerDay = monthlySalaryWithTax / workingDaysInMonth;
        
        totalCost += monthCostPerDay * daysInProject;
    }

    return totalCost;
}
}