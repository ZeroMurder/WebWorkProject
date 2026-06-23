using WebWorkNew.Models;

namespace WebWorkNew.Services;

public interface ITechnicalTaskService
{
    Task<TechnicalTask?> GetByProjectIdAsync(int projectId);
    Task<TechnicalTask> CreateOrUpdateAsync(TechnicalTask technicalTask);
    Task<string> GenerateHtmlAsync(TechnicalTask technicalTask);
    Task<byte[]> GeneratePdfAsync(TechnicalTask technicalTask);
    Task<byte[]> GenerateWordAsync(TechnicalTask technicalTask);
    Task<bool> ValidateAsync(TechnicalTask technicalTask);
}