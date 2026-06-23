using WebWorkNew.Models;
using WebWorkNew.Data;

namespace WebWorkNew.Data;

public static class HolidaySeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.Holidays.Any()) return;

        var holidays = new List<Holiday>
        {
            // Новогодние праздники
            new Holiday(new DateTime(2024, 1, 1), "Новый год"),
            new Holiday(new DateTime(2024, 1, 2), "Новогодние каникулы"),
            new Holiday(new DateTime(2024, 1, 3), "Новогодние каникулы"),
            new Holiday(new DateTime(2024, 1, 4), "Новогодние каникулы"),
            new Holiday(new DateTime(2024, 1, 5), "Новогодние каникулы"),
            new Holiday(new DateTime(2024, 1, 6), "Новогодние каникулы"),
            new Holiday(new DateTime(2024, 1, 7), "Рождество Христово"),
            new Holiday(new DateTime(2024, 1, 8), "Новогодние каникулы"),
            
            // Другие праздники
            new Holiday(new DateTime(2024, 2, 23), "День защитника Отечества"),
            new Holiday(new DateTime(2024, 3, 8), "Международный женский день"),
            new Holiday(new DateTime(2024, 5, 1), "Праздник Весны и Труда"),
            new Holiday(new DateTime(2024, 5, 9), "День Победы"),
            new Holiday(new DateTime(2024, 6, 12), "День России"),
            new Holiday(new DateTime(2024, 11, 4), "День народного единства"),
            
            // Для 2025 года
            new Holiday(new DateTime(2025, 1, 1), "Новый год"),
            new Holiday(new DateTime(2025, 1, 2), "Новогодние каникулы"),
            new Holiday(new DateTime(2025, 1, 3), "Новогодние каникулы"),
            new Holiday(new DateTime(2025, 1, 4), "Новогодние каникулы"),
            new Holiday(new DateTime(2025, 1, 5), "Новогодние каникулы"),
            new Holiday(new DateTime(2025, 1, 6), "Новогодние каникулы"),
            new Holiday(new DateTime(2025, 1, 7), "Рождество Христово"),
            new Holiday(new DateTime(2025, 1, 8), "Новогодние каникулы"),
            new Holiday(new DateTime(2025, 2, 23), "День защитника Отечества"),
            new Holiday(new DateTime(2025, 3, 8), "Международный женский день"),
            new Holiday(new DateTime(2025, 5, 1), "Праздник Весны и Труда"),
            new Holiday(new DateTime(2025, 5, 9), "День Победы"),
            new Holiday(new DateTime(2025, 6, 12), "День России"),
            new Holiday(new DateTime(2025, 11, 4), "День народного единства"),
        };

        context.Holidays.AddRange(holidays);
        context.SaveChanges();
    }
}