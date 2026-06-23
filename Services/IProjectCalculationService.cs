using WebWorkNew.Models;

namespace WebWorkNew.Services;

public interface IProjectCalculationService
{
    Task RecalculateAsync(Project project);
}
