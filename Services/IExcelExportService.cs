using WebWorkNew.Models;

namespace WebWorkNew.Services;

public interface IExcelExportService
{
    byte[] ExportProjectsToExcel(List<Project> projects);
    byte[] ExportResourcesToExcel(List<ProjectResource> resources);
    byte[] ExportCommercialOfferToExcel(Project project);
    byte[] ExportNmaToExcel(Project project);
    byte[] ExportWorkspaceUsersToExcel(Workspace workspace, List<WorkspaceUser> users);
}