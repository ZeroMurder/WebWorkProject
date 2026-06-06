using WebWork.Models;

namespace WebWork.Services;

public interface IProjectCalculationService
{
    Task RecalculateAsync(Project project);
}
