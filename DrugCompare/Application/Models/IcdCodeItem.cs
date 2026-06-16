namespace DrugCompare.Application.Models;

public sealed class IcdCodeItem
{
    public long Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Chapter { get; set; } = string.Empty;

    public string? ParentCode { get; set; }
}