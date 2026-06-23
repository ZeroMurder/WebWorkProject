using System.ComponentModel.DataAnnotations;
using WebWorkNew.Enums;

namespace WebWorkNew.ViewModels.CustomerViewModels;

public class CustomerViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите ИНН")]
    [Display(Name = "ИНН")]
    [StringLength(12, MinimumLength = 10, ErrorMessage = "ИНН должен содержать 10 или 12 цифр")]
    public string Inn { get; set; } = "";

    [Required(ErrorMessage = "Выберите тип заказчика")]
    [Display(Name = "Тип заказчика")]
    public CustomerType Type { get; set; }

    [Display(Name = "Название организации")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Введите ФИО")]
    [Display(Name = "ФИО")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Введите email")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Введите телефон")]
    [Phone(ErrorMessage = "Некорректный формат телефона")]
    [Display(Name = "Телефон")]
    public string Phone { get; set; } = "";

    public string TypeDisplayName => Type switch
    {
        CustomerType.PhysicalPerson => "Физическое лицо",
        CustomerType.IndividualEntrepreneur => "Индивидуальный предприниматель",
        CustomerType.LegalEntity => "Юридическое лицо",
        _ => "Неизвестно"
    };
}

