namespace WebWorkNew.Models;

public class Project
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Description { get; set; }
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public decimal TaxRate { get; set; }

    public decimal TotalCostWithoutMargin { get; set; }
    public decimal TotalCostWithMargin { get; set; }
    public decimal NetProfit { get; set; }

    public List<ProjectResource> Resources { get; set; } = new();
    
    // Привязка к рабочей области
    public int? WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }
    
    // Статус проекта
    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
}

public enum ProjectStatus
{
    Draft,      // Черновик
    Active,     // Активный
    Paused,     // Приостановлен
    Completed,  // Завершен
    Cancelled   // Отменен
}