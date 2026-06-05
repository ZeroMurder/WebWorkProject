namespace WebWork.Models;

public class Employee
{
    public int Id { get; set; }
    public string Surname { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Patronymic { get; set; }
    public string Position { get; set; } = "";
    public decimal MonthlySalary { get; set; }
    public decimal TaxRate { get; set; }

    public decimal CalculateCost(int workingDaysInMonth, int workedDays)
    {
        if (workingDaysInMonth <= 0) return 0;
        var dayRate = (MonthlySalary + MonthlySalary * TaxRate / 100m) / workingDaysInMonth;
        return dayRate * workedDays;
    }
}