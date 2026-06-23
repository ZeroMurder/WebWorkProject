using Microsoft.AspNetCore.Identity;

namespace WebWorkNew.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? MiddleName { get; set; }
    public string? Position { get; set; }
}