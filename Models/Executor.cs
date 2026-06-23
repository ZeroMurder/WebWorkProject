using WebWorkNew.Enums;

namespace WebWorkNew.Models;

public class Executor
{
    public int Id { get; set; }
    public string Surname { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Patronymic { get; set; }
    
    public string FullName => $"{Surname} {Name} {Patronymic}".Trim();
    
    public EmploymentType EmploymentType { get; set; }
    public decimal TaxRate { get; set; }
    public TimeUnit Unit { get; set; }
    public decimal CostPerUnit { get; set; }

    // Базовый метод (использует TaxRate из модели)
    public decimal CalculateCost(decimal units)
    {
        var baseCost = units * CostPerUnit;
        return baseCost + baseCost * TaxRate / 100m;
    }

    // Перегрузка с параметром taxRate (для НПД)
    public decimal CalculateCost(decimal units, decimal taxRate)
    {
        var baseCost = units * CostPerUnit;
        return baseCost + baseCost * taxRate / 100m;
    }
}