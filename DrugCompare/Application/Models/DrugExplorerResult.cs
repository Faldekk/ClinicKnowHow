namespace DrugCompare.Application.Models;

public class DrugExplorerResult
{
    public long DrugId { get; set; }
    public string DrugName { get; set; } = "";
    public string NormalizedName { get; set; } = "";
    public string? Manufacturer { get; set; }
    public string? Source { get; set; }
    public string ActiveSubstances { get; set; } = "";
    public int ActiveSubstanceCount { get; set; }
}
