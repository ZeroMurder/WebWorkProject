using WebWorkNew.Enums;

namespace WebWorkNew.Models;

public class Equipment
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public EquipmentAcquisitionType AcquisitionType { get; set; }
    public decimal? OperationalCost { get; set; }
    public TimeUnit Unit { get; set; }
    public decimal CostPerUnit { get; set; }

    public decimal CalculateCost(decimal units) => units * CostPerUnit;
}