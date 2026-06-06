using Microsoft.EntityFrameworkCore;
using WebWork.Models;
using WebWork.Enums;
using WebWork.Services;

namespace WebWork.Data;

public static class TestDataSeeder
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            // Проверяем подключение к БД
            var canConnect = await db.Database.CanConnectAsync();
            if (!canConnect)
            {
                logger.LogWarning("Cannot connect to database, skipping seed");
                return;
            }

            // Если уже есть проекты — не пересоздаём
            if (await db.Projects.AnyAsync())
            {
                logger.LogInformation("Database already has projects, skipping seed");
                return;
            }

            logger.LogInformation("Seeding test data...");

            // Customers
            var c1 = new Customer
            {
                Inn = "7707083893",
                Type = CustomerType.LegalEntity,
                Name = "ООО Ромашка",
                FullName = "Иванов И.И.",
                Email = "buyer@example.com",
                Phone = "+79990000000"
            };

            var c2 = new Customer
            {
                Inn = "1234567890",
                Type = CustomerType.IndividualEntrepreneur,
                FullName = "Петров П.П.",
                Email = "ip@example.com",
                Phone = "+79990000011"
            };

            db.Customers.AddRange(c1, c2);
            await db.SaveChangesAsync();

            // Employees
            var e1 = new Employee
            {
                Surname = "Смирнов",
                Name = "Алексей",
                Patronymic = "Игоревич",
                Position = "Разработчик",
                MonthlySalary = 200000,
                TaxRate = 30.2m
            };
            
            var e2 = new Employee
            {
                Surname = "Кузнецова",
                Name = "Мария",
                Patronymic = null,
                Position = "QA",
                MonthlySalary = 120000,
                TaxRate = 7.6m
            };
            
            db.Employees.AddRange(e1, e2);
            await db.SaveChangesAsync();

            // Executors (physical persons)
            var ex1 = new Executor
            {
                Surname = "Сидоров",
                Name = "Дмитрий",
                Patronymic = null,
                EmploymentType = EmploymentType.FixedSalary,
                TaxRate = 7.6m,
                Unit = TimeUnit.Hours,
                CostPerUnit = 2500m
            };
            
            db.Executors.Add(ex1);
            await db.SaveChangesAsync();

            // Subcontractor (legal)
            var s1 = new Subcontractor
            {
                Inn = "7800000000",
                Name = "ООО ТехИнтеграция",
                ContactName = "Руководитель",
                Email = "sub@example.com",
                Phone = "+79990000222",
                Unit = TimeUnit.Days,
                CostPerUnit = 45000m,
                TaxRate = 30.2m
            };
            
            db.Subcontractors.Add(s1);
            await db.SaveChangesAsync();

            // Equipment
            var eq1 = new Equipment
            {
                Title = "Сервер",
                Description = "Аренда оборудования",
                AcquisitionType = EquipmentAcquisitionType.Rental,
                OperationalCost = 50000m,
                Unit = TimeUnit.FullCost,
                CostPerUnit = 300000m
            };
            
            db.Equipments.Add(eq1);
            await db.SaveChangesAsync();

            // Project
            var p1 = new Project
            {
                Title = "IT-Разработка — тестовый проект",
                StartDate = DateTime.Today.AddDays(-10),
                EndDate = DateTime.Today.AddDays(20),
                Description = "Тестовые данные для сдачи",
                CustomerId = c1.Id,
                TaxRate = 20m,
                TotalCostWithoutMargin = 0,
                TotalCostWithMargin = 0,
                NetProfit = 0
            };
            
            db.Projects.Add(p1);
            await db.SaveChangesAsync();

            // Resources
            var r1 = new ProjectResource
            {
                ProjectId = p1.Id,
                ResourceName = "Разработка",
                Type = ResourceType.Employee,
                EmployeeId = e1.Id,
                ServiceName = "Разработка ПО",
                StartDate = p1.StartDate,
                EndDate = p1.EndDate,
                UnitsCount = 10,
                MarginPercent = 20m
            };

            var r2 = new ProjectResource
            {
                ProjectId = p1.Id,
                ResourceName = "Тестирование",
                Type = ResourceType.Executor,
                ExecutorId = ex1.Id,
                ServiceName = "QA услуги",
                StartDate = p1.StartDate,
                EndDate = p1.EndDate,
                UnitsCount = 8,
                MarginPercent = 15m
            };

            var r3 = new ProjectResource
            {
                ProjectId = p1.Id,
                ResourceName = "Интеграция",
                Type = ResourceType.Subcontractor,
                SubcontractorId = s1.Id,
                ServiceName = "Внешняя интеграция",
                StartDate = p1.StartDate,
                EndDate = p1.EndDate,
                UnitsCount = 5,
                MarginPercent = 10m
            };

            var r4 = new ProjectResource
            {
                ProjectId = p1.Id,
                ResourceName = "Инфраструктура",
                Type = ResourceType.Equipment,
                EquipmentId = eq1.Id,
                ServiceName = "Использование оборудования",
                StartDate = p1.StartDate,
                EndDate = p1.EndDate,
                UnitsCount = 1,
                MarginPercent = 0m
            };

            db.ProjectResources.AddRange(r1, r2, r3, r4);
            await db.SaveChangesAsync();

            // Пересчёт итогов
            p1.Resources = new List<ProjectResource> { r1, r2, r3, r4 };
            
            var calculator = new ProjectCalculationService(db);
            await calculator.RecalculateAsync(p1);
            await db.SaveChangesAsync();

            logger.LogInformation("Test data seeded successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding test data");
        }
    }
}