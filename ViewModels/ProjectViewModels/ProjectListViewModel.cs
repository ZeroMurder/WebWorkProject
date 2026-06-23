using WebWorkNew.Models;

namespace WebWorkNew.ViewModels.ProjectViewModels;

public class ProjectListViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; }

    public decimal TotalCostWithoutMargin { get; set; }
    public decimal TotalCostWithMargin { get; set; }
    public decimal NetProfit { get; set; }

    public Customer? Customer { get; set; }
    public int ResourceCount { get; set; }

    // Для отображения статуса цветом
    public string StatusBadgeClass => GetStatusBadgeClass();

    private string GetStatusBadgeClass()
    {
        return Status switch
        {
            ProjectStatus.Draft => "bg-secondary",
            ProjectStatus.Active => "bg-success",
            ProjectStatus.Paused => "bg-warning text-dark",
            ProjectStatus.Completed => "bg-info",
            ProjectStatus.Cancelled => "bg-danger",
            _ => "bg-secondary"
        };
    }
}

