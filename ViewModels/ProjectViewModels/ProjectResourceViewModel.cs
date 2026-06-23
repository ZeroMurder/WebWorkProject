using WebWorkNew.Enums;

namespace WebWorkNew.ViewModels.ProjectViewModels;

public class ProjectResourceViewModel
{
    public int Id { get; set; }
    public int ProjectId { get; set; }

    public string ResourceName { get; set; } = "";
    public ResourceType Type { get; set; }

    public string? ServiceName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public decimal UnitsCount { get; set; }
    public decimal CostPrice { get; set; }
    public decimal MarginPercent { get; set; }
    public decimal FinalCost { get; set; }

    // Исполнитель/связанные справочники (в зависимости от Type)
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }

    public int? ExecutorId { get; set; }
    public string? ExecutorName { get; set; }

    public int? SubcontractorId { get; set; }
    public string? SubcontractorName { get; set; }

    public int? EquipmentId { get; set; }
    public string? EquipmentName { get; set; }

    public string ExecutorDisplayName => GetExecutorDisplayName();

    private string GetExecutorDisplayName()
    {
        if (!string.IsNullOrEmpty(EmployeeName)) return EmployeeName;
        if (!string.IsNullOrEmpty(ExecutorName)) return ExecutorName;
        if (!string.IsNullOrEmpty(SubcontractorName)) return SubcontractorName;
        if (!string.IsNullOrEmpty(EquipmentName)) return EquipmentName;
        return "—";
    }

    public string TypeDisplayName => Type.ToString();

    // Для маржинальности
    public bool CanEditMargin { get; set; }
}

