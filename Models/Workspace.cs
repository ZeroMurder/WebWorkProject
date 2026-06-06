namespace WebWork.Models;

public class Workspace
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Subdomain { get; set; } = "";
    public string? AdminUserId { get; set; }
}