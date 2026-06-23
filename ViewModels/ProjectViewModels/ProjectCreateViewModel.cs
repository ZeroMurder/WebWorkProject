using System.ComponentModel.DataAnnotations;
using WebWorkNew.Models;

namespace WebWorkNew.ViewModels.ProjectViewModels;

public class ProjectCreateViewModel
{
    [Required(ErrorMessage = "Введите название проекта")]
    [Display(Name = "Название проекта")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Укажите дату начала")]
    [Display(Name = "Дата начала")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Укажите дату окончания")]
    [Display(Name = "Дата окончания")]
    public DateTime EndDate { get; set; }

    [Display(Name = "Описание")]
    public string? Description { get; set; }

    [Display(Name = "Заказчик")]
    public int? CustomerId { get; set; }

    [Required(ErrorMessage = "Укажите налоговую ставку")]
    [Range(0, 100, ErrorMessage = "Налоговая ставка должна быть от 0 до 100%")]
    [Display(Name = "Налоговая ставка (%)")]
    public decimal TaxRate { get; set; }

    [Display(Name = "Статус")]
    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;

    // Для отображения в UI
    public List<Customer>? Customers { get; set; }
    public bool CanEditMargin { get; set; }

    // Для связки с рабочей областью (используется в текущих контроллерах)
    public int? WorkspaceId { get; set; }
}

