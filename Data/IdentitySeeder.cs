using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WebWorkNew.Models;

namespace WebWorkNew.Data;

public static class IdentitySeeder
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = ["GlobalAdmin", "CommercialDirector", "Accountant", "HR"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var adminEmail = "admin@local";
        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "Main",
                MiddleName = "",
                Position = "Global Admin",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "GlobalAdmin");
                await userManager.AddToRoleAsync(admin, "HR");
                await userManager.AddToRoleAsync(admin, "CommercialDirector");
                await userManager.AddToRoleAsync(admin, "Accountant");
                
                // Добавляем Claims
                await userManager.AddClaimAsync(admin, new Claim("FirstName", admin.FirstName));
                await userManager.AddClaimAsync(admin, new Claim("LastName", admin.LastName));
                await userManager.AddClaimAsync(admin, new Claim("Position", admin.Position));
            }
        }
        else
        {
            // Обновляем существующего пользователя
            await userManager.RemovePasswordAsync(admin);
            await userManager.AddPasswordAsync(admin, "Admin123!");

            if (!await userManager.IsInRoleAsync(admin, "GlobalAdmin"))
                await userManager.AddToRoleAsync(admin, "GlobalAdmin");
            if (!await userManager.IsInRoleAsync(admin, "HR"))
                await userManager.AddToRoleAsync(admin, "HR");
            if (!await userManager.IsInRoleAsync(admin, "CommercialDirector"))
                await userManager.AddToRoleAsync(admin, "CommercialDirector");
            if (!await userManager.IsInRoleAsync(admin, "Accountant"))
                await userManager.AddToRoleAsync(admin, "Accountant");
            
            // Добавляем Claims, если их нет
            var claims = await userManager.GetClaimsAsync(admin);
            if (!claims.Any(c => c.Type == "FirstName"))
                await userManager.AddClaimAsync(admin, new Claim("FirstName", admin.FirstName ?? ""));
            if (!claims.Any(c => c.Type == "LastName"))
                await userManager.AddClaimAsync(admin, new Claim("LastName", admin.LastName ?? ""));
            if (!claims.Any(c => c.Type == "Position"))
                await userManager.AddClaimAsync(admin, new Claim("Position", admin.Position ?? ""));
        }
    }
}