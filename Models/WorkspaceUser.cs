namespace WebWorkNew.Models;

public class WorkspaceUser
{
    public int Id { get; set; }
    // public int Admin { get; set; }   // ← УДАЛИТЬ
    public int WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }
    public string UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }
    public bool CanView { get; set; } = true;
    public bool CanEditProjects { get; set; } = false;
    public bool CanManageWorkspace { get; set; } = false;
}