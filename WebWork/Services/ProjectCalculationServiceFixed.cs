using Microsoft.EntityFrameworkCore;
using WebWork.Data;
using WebWork.Enums;
using WebWork.Models;

namespace WebWork.Services;

public class ProjectCalculationServiceFixed : IProjectCalculationService
{
    private readonly AppDbContext _db;

    public ProjectCalculationServiceFixed(AppDbContext db) => _db = db;

    public async Task RecalculateAsync(Project project)
    {
        decimal totalBase = 0;
        decimal totalFinal = 0;
        decimal profit = 0;

        foreach (var r in project.Resources)
        {
            decimal cost = 0;

            if (r.Type == ResourceType.Employee && r.EmployeeId.HasValue)
            {
                var emp = await _db.Employees.FindAsync(r.EmployeeId.Value);
                if (emp != null)
                {
                    var monthDays = DateTime.DaysInMonth(r.StartDate.Year, r.StartDate.Month);
                    cost = emp.CalculateCost(monthDays, (int)r.UnitsCount);
                }
            }
            else if (r.Type == ResourceType.Executor && r.ExecutorId.HasValue)
            {
                var ex = await _db.Executors.FindAsync(r.ExecutorId.Value);
                if (ex != null)
                    cost = ex.CalculateCost(r.UnitsCount);
            }
            else if (r.Type == ResourceType.Subcontractor && r.SubcontractorId.HasValue)
            {
                var s = await _db.Subcontractors.FindAsync(r.SubcontractorId.Value);
                if (s != null)
                    cost = s.CalculateCost(r.UnitsCount);
            }
            else if (r.Type == ResourceType.Equipment && r.EquipmentId.HasValue)
            {
                var eq = await _db.Equipments.FindAsync(r.EquipmentId.Value);
                if (eq != null)
                    cost = eq.CalculateCost(r.UnitsCount);
            }

            r.CostPrice = cost;
            r.FinalCost = cost + cost * r.MarginPercent / 100m;

            totalBase += r.CostPrice;
            totalFinal += r.FinalCost;
            profit += r.FinalCost - r.CostPrice;
        }

        project.TotalCostWithoutMargin = totalBase;
        project.TotalCostWithMargin = totalFinal + totalFinal * project.TaxRate / 100m;
        project.NetProfit = profit;
    }
}

