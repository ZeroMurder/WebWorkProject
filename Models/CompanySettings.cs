namespace WebWorkNew.Models;

public class CompanySettings
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = "";
    public string DirectorFullName { get; set; } = "";
    public string DirectorPosition { get; set; } = "";
    public string? LogoPath { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}