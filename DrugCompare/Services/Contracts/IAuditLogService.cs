using DrugCompare.Models;

namespace DrugCompare.Services.Contracts;

public interface IAuditLogService
{
    Task WriteAsync(string action, object? details = null);

    Task<List<AuditLogItem>> GetRecentAsync(int limit = 100);
}