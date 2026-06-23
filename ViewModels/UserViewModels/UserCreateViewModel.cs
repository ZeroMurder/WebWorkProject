using System.ComponentModel.DataAnnotations;

namespace WebWorkNew.ViewModels.UserViewModels;

public class UserCreateViewModel
{
    [Required]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = "";

    [Required]
    [Display(Name = "Имя")]
    public string FirstName { get; set; } = "";

    [Display(Name = "Отчество")]
    public string? MiddleName { get; set; }

    [Required]
    [Display(Name = "Email")]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    [Display(Name = "Должность")]
    public string Position { get; set; } = "";

    [Required]
    [Display(Name = "Пароль")]
    [StringLength(50, MinimumLength = 4)]
    public string Password { get; set; } = "";

    public string? Role { get; set; }
    public List<int>? WorkspaceIds { get; set; }
}

