using DrugCompare.Application.Models;

namespace DrugCompare.Application.Services.Contracts;

public interface IAuditLogService
{
    Task<List<AuditLogItem>> GetRecentLogsAsync(int limit = 50);
}