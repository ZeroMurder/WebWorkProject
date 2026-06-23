using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebWorkNew.Models;

namespace WebWorkNew.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>

{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Основные реестры
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Executor> Executors { get; set; }
    public DbSet<Subcontractor> Subcontractors { get; set; }
    public DbSet<Equipment> Equipments { get; set; }

    // Проекты и ресурсы
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectResource> ProjectResources { get; set; }

    // Настройки компании
    public DbSet<CompanySettings> CompanySettings { get; set; }

    // Профиль/Аватар/Аудит
    public DbSet<UserAvatar> UserAvatars { get; set; }
    public DbSet<UserAuditLog> UserAuditLogs { get; set; }


    // Рабочие области
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<WorkspaceUser> WorkspaceUsers { get; set; }

    // Техническое задание
    public DbSet<TechnicalTask> TechnicalTasks { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Holiday> Holidays { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Настройка уникальности поддомена для рабочей области
        builder.Entity<Workspace>()
            .HasIndex(w => w.Subdomain)
            .IsUnique();

        // Связи WorkspaceUser
        builder.Entity<WorkspaceUser>()
            .HasOne(wu => wu.Workspace)
            .WithMany(w => w.Users)
            .HasForeignKey(wu => wu.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<WorkspaceUser>()
            .HasOne(wu => wu.User)
            .WithMany()
            .HasForeignKey(wu => wu.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Связи TechnicalTask
        builder.Entity<TechnicalTask>()
            .HasOne(tt => tt.Project)
            .WithMany()
            .HasForeignKey(tt => tt.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Связи Service (типовые услуги для реестров)
        builder.Entity<Service>()
            .HasOne(s => s.Employee)
            .WithMany()
            .HasForeignKey(s => s.EmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Service>()
            .HasOne(s => s.Executor)
            .WithMany()
            .HasForeignKey(s => s.ExecutorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Service>()
            .HasOne(s => s.Subcontractor)
            .WithMany()
            .HasForeignKey(s => s.SubcontractorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Service>()
            .HasOne(s => s.Equipment)
            .WithMany()
            .HasForeignKey(s => s.EquipmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}