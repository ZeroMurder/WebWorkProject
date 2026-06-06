using WebWork.Enums;

namespace WebWork.Models;

public class ProjectResource
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    public string ResourceName { get; set; } = "";
    public ResourceType Type { get; set; }

    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int? ExecutorId { get; set; }
    public Executor? Executor { get; set; }

    public int? SubcontractorId { get; set; }
    public Subcontractor? Subcontractor { get; set; }

    public int? EquipmentId { get; set; }
    public Equipment? Equipment { get; set; }

    public string? ServiceName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public decimal UnitsCount { get; set; }
    public decimal CostPrice { get; set; }
    public decimal MarginPercent { get; set; }
    public decimal FinalCost { get; set; }
}