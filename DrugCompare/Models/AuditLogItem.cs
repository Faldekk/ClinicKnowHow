namespace DrugCompare.Models;

public sealed class AuditLogItem
{
    public long Id { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? DetailsJson { get; set; }

    public DateTime CreatedAt { get; set; }
}