using WebWork.Enums;

namespace WebWork.Models;

public class Subcontractor
{
    public int Id { get; set; }
    public string Inn { get; set; } = "";
    public string Name { get; set; } = "";
    public string ContactName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public TimeUnit Unit { get; set; }
    public decimal CostPerUnit { get; set; }
    public decimal TaxRate { get; set; }

    public decimal CalculateCost(decimal units)
    {
        var baseCost = units * CostPerUnit;
        return baseCost + baseCost * TaxRate / 100m;
    }
}