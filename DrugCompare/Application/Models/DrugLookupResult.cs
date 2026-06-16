namespace DrugCompare.Application.Models;

public sealed class DrugLookupResult
{
    public string DrugName { get; set; } = string.Empty;

    public List<ActiveSubstanceItem> ActiveSubstances { get; set; } = new();
}
