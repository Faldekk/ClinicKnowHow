using DrugCompare.Models;

namespace DrugCompare.Repositories.Contracts;

public interface IAuditLogRepository
{
    Task WriteAsync(string action, string? detailsJson = null);

    Task<List<AuditLogItem>> GetRecentAsync(int limit = 100);
}