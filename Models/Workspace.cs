namespace WebWorkNew.Models;

public class Workspace
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Subdomain { get; set; } = "";   // например, "workspace1"
    public string? AdminUserId { get; set; }      // ID пользователя-администратора рабочей области
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<WorkspaceUser> Users { get; set; } = new();
}