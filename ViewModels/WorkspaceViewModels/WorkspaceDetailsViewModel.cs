using WebWorkNew.Models;

namespace WebWorkNew.ViewModels.WorkspaceViewModels;

public class WorkspaceDetailsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Subdomain { get; set; } = "";

    public string? AdminUserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<WorkspaceUser> Users { get; set; } = new();
    public List<Project> Projects { get; set; } = new();

    public bool CanEditProjects { get; set; }
    public bool CanManageWorkspace { get; set; }
    public bool IsAdmin { get; set; }

    public int UserCount => Users.Count;
    public int ProjectCount => Projects.Count;
}

