namespace DrugCompare.Models;

public sealed class ActiveSubstanceItem
{
    public long? DatabaseId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public string? DDInterId { get; set; }

    public string Source { get; set; } = "Manual";

    public string DisplayName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(DDInterId))
                return $"{Name} ({DDInterId})";

            return Name;
        }
    }
}