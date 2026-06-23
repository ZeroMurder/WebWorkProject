using System.ComponentModel.DataAnnotations;

namespace WebWorkNew.ViewModels.EmployeeViewModels;

public class EmployeeViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите фамилию")]
    [Display(Name = "Фамилия")]
    public string Surname { get; set; } = "";

    [Required(ErrorMessage = "Введите имя")]
    [Display(Name = "Имя")]
    public string Name { get; set; } = "";

    [Display(Name = "Отчество")]
    public string? Patronymic { get; set; }

    [Required(ErrorMessage = "Введите должность")]
    [Display(Name = "Должность")]
    public string Position { get; set; } = "";

    [Required(ErrorMessage = "Введите оклад")]
    [Range(0, double.MaxValue, ErrorMessage = "Оклад должен быть положительным числом")]
    [Display(Name = "Оклад в месяц")]
    [DataType(DataType.Currency)]
    public decimal MonthlySalary { get; set; }

    [Required(ErrorMessage = "Введите налоговую ставку")]
    [Range(0, 100, ErrorMessage = "Ставка должна быть от 0 до 100%")]
    [Display(Name = "Налоговая ставка (%)")]
    public decimal TaxRate { get; set; }

    public string FullName => $"{Surname} {Name} {Patronymic}".Trim();
}

