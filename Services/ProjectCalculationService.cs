using Microsoft.EntityFrameworkCore;
using WebWork.Data;
using WebWork.Enums;
using WebWork.Models;

namespace WebWork.Services;

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
        project.TotalCostWithMargin = totalFinal + totalFinal * project.TaxRate / 100m;
        project.NetProfit = profit;
    }

    private async Task<decimal> CalculateResourceCost(ProjectResource resource)
    {
        if (resource == null) return 0;
        
        switch (resource.Type)
        {
            case ResourceType.Employee when resource.EmployeeId.HasValue:
                var emp = await _db.Employees.FindAsync(resource.EmployeeId.Value);
                if (emp != null)
                {
                    var workingDays = GetWorkingDaysInPeriod(resource.StartDate, resource.EndDate);
                    return emp.CalculateCost(workingDays, (int)resource.UnitsCount);
                }
                break;

            case ResourceType.Executor when resource.ExecutorId.HasValue:
                var ex = await _db.Executors.FindAsync(resource.ExecutorId.Value);
                if (ex != null)
                    return ex.CalculateCost(resource.UnitsCount);
                break;

            case ResourceType.Subcontractor when resource.SubcontractorId.HasValue:
                var sub = await _db.Subcontractors.FindAsync(resource.SubcontractorId.Value);
                if (sub != null)
                    return sub.CalculateCost(resource.UnitsCount);
                break;

            case ResourceType.Equipment when resource.EquipmentId.HasValue:
                var eq = await _db.Equipments.FindAsync(resource.EquipmentId.Value);
                if (eq != null)
                    return eq.CalculateCost(resource.UnitsCount);
                break;
        }
        
        return 0;
    }

    private int GetWorkingDaysInPeriod(DateTime start, DateTime end)
    {
        var days = 0;
        for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
        {
            if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                days++;
        }
        return days > 0 ? days : 1;
    }
}