using DrugCompare.Models;
using DrugCompare.Repositories.Contracts;
using DrugCompare.Services.Contracts;
using System.Text.Json;

namespace DrugCompare.Services.Application;

public sealed class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task WriteAsync(string action, object? details = null)
    {
        string? detailsJson = null;

        if (details is not null)
        {
            detailsJson = JsonSerializer.Serialize(details, new JsonSerializerOptions
            {
                WriteIndented = false
            });
        }

        await _auditLogRepository.WriteAsync(action, detailsJson);
    }

    public Task<List<AuditLogItem>> GetRecentAsync(int limit = 100)
    {
        return _auditLogRepository.GetRecentAsync(limit);
    }
}