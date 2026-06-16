using System.Text.Json;
using DrugCompare.Application.Models;
using DrugCompare.Application.Repositories.Contracts;
using DrugCompare.Application.Services.Contracts;

namespace DrugCompare.Application.Services.Implementations;

public sealed class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;

    public AuditLogService(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public Task AddAsync(string eventType, string? details = null)
    {
        return _repository.AddAsync(eventType, details);
    }

    public Task WriteAsync(string eventType, string? details = null)
    {
        return AddAsync(eventType, details);
    }

    public Task WriteAsync(string eventType, object? details)
    {
        var detailsJson = details is null
            ? null
            : JsonSerializer.Serialize(details);

        return AddAsync(eventType, detailsJson);
    }

    public Task<List<AuditLogItem>> GetRecentLogsAsync(int limit = 50)
    {
        return _repository.GetRecentLogsAsync(limit);
    }

    public Task<List<AuditLogItem>> GetRecentAsync(int limit = 50)
    {
        return GetRecentLogsAsync(limit);
    }
}