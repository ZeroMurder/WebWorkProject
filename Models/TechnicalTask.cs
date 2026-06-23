using System.ComponentModel.DataAnnotations;

namespace WebWorkNew.Models;

public class TechnicalTask
{
    public int Id { get; set; }
    
    public int ProjectId { get; set; }
    public Project? Project { get; set; }
    
    [Required(ErrorMessage = "Введите название ТЗ")]
    [Display(Name = "Название технического задания")]
    public string Title { get; set; } = "";
    
    [Display(Name = "Версия")]
    public string Version { get; set; } = "1.0";
    
    [Display(Name = "Цели и задачи")]
    public string Goals { get; set; } = "";
    
    [Display(Name = "Функциональные требования")]
    public string FunctionalRequirements { get; set; } = "";
    
    [Display(Name = "Нефункциональные требования")]
    public string NonFunctionalRequirements { get; set; } = "";
    
    [Display(Name = "Состав системы")]
    public string SystemComposition { get; set; } = "";
    
    [Display(Name = "Технологический стек")]
    public string TechStack { get; set; } = "";
    
    [Display(Name = "Требования к интерфейсу")]
    public string UiRequirements { get; set; } = "";
    
    [Display(Name = "Состав документации")]
    public string Documentation { get; set; } = "";
    
    [Display(Name = "Примечания")]
    public string? Notes { get; set; }
    
    [Display(Name = "Дата создания")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    [Display(Name = "Дата обновления")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    [Display(Name = "Статус")]
    public TechnicalTaskStatus Status { get; set; } = TechnicalTaskStatus.Draft;
}

public enum TechnicalTaskStatus
{
    [Display(Name = "Черновик")]
    Draft,
    
    [Display(Name = "На согласовании")]
    UnderReview,
    
    [Display(Name = "Согласовано")]
    Approved,
    
    [Display(Name = "В работе")]
    InProgress,
    
    [Display(Name = "Завершено")]
    Completed
}