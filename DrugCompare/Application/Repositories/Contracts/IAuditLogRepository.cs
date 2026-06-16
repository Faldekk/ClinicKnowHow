using DrugCompare.Application.Models;

namespace DrugCompare.Application.Repositories.Contracts;

public interface IAuditLogRepository
{
    Task AddAsync(string eventType, string? details = null);
    Task<List<AuditLogItem>> GetRecentLogsAsync(int limit = 50);
}