namespace WebWork.Models;

public class WorkspaceUser
{
    public int Id { get; set; }

    public int WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }

    // Identity user id (AspNetUsers.Id)
    public string UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }

    public bool CanView { get; set; }
    public bool CanEditProjects { get; set; }
    public bool CanManageWorkspace { get; set; }
}

