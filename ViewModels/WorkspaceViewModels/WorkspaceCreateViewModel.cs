using System.ComponentModel.DataAnnotations;

namespace WebWorkNew.ViewModels.WorkspaceViewModels;

public class WorkspaceCreateViewModel
{
    [Required(ErrorMessage = "Введите название рабочей области")]
    [Display(Name = "Название")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Введите поддомен")]
    [Display(Name = "Поддомен")]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Только строчные буквы, цифры и дефис")]
    public string Subdomain { get; set; } = "";
}

