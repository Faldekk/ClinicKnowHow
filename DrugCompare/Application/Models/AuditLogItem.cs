namespace DrugCompare.Application.Models;

public sealed class AuditLogItem
{
    public long Id { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string? Details { get; set; }

    public string? DetailsJson { get; set; }

    public DateTime CreatedAt { get; set; }
}