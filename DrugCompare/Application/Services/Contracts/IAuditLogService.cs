using DrugCompare.Application.Models;

namespace DrugCompare.Application.Services.Contracts;

public interface IAuditLogService
{
    Task AddAsync(string eventType, string? details = null);

    Task WriteAsync(string eventType, string? details = null);

    Task WriteAsync(string eventType, object? details);

    Task<List<AuditLogItem>> GetRecentLogsAsync(int limit = 50);

    Task<List<AuditLogItem>> GetRecentAsync(int limit = 50);
}