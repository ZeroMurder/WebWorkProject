using System.ComponentModel.DataAnnotations;
using WebWorkNew.Enums;

namespace WebWorkNew.Models;

public class Service
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Введите название услуги")]
    [Display(Name = "Название услуги")]
    public string Name { get; set; } = "";
    
    [Display(Name = "Описание")]
    public string? Description { get; set; }
    
    [Display(Name = "Тип ресурса")]
    public ResourceType ResourceType { get; set; }
    
    // Связи с ресурсами
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    
    public int? ExecutorId { get; set; }
    public Executor? Executor { get; set; }
    
    public int? SubcontractorId { get; set; }
    public Subcontractor? Subcontractor { get; set; }
    
    public int? EquipmentId { get; set; }
    public Equipment? Equipment { get; set; }
    
    [Display(Name = "Единица измерения")]
    public TimeUnit Unit { get; set; }
    
    [Display(Name = "Стоимость за единицу")]
    [DataType(DataType.Currency)]
    public decimal CostPerUnit { get; set; }
    
    [Display(Name = "Налоговая ставка (%)")]
    public decimal TaxRate { get; set; }
    
    [Display(Name = "Активно")]
    public bool IsActive { get; set; } = true;
    
    [Display(Name = "Стандартная маржинальность (%)")]
    public decimal DefaultMarginPercent { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}