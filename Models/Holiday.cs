// Models/Holiday.cs

namespace WebWorkNew.Models;

public class Holiday
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string? Name { get; set; }
    public int Year { get; set; }
    
    public Holiday() { }
    
    public Holiday(DateTime date, string? name = null)
    {
        Date = date;
        Name = name;
        Year = date.Year;
    }
}