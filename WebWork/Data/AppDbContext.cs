using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebWork.Models;

namespace WebWork.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Executor> Executors => Set<Executor>();
    public DbSet<Subcontractor> Subcontractors => Set<Subcontractor>();
    public DbSet<Equipment> Equipments => Set<Equipment>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectResource> ProjectResources => Set<ProjectResource>();
    public DbSet<CompanySettings> CompanySettings => Set<CompanySettings>();
}