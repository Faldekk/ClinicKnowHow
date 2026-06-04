namespace DrugCompare.Models;

public sealed class InteractionResult
{
    public string SubstanceA { get; set; } = string.Empty;

    public string SubstanceB { get; set; } = string.Empty;

    public string Severity { get; set; } = "Unknown";

    public string Message { get; set; } = string.Empty;

    public string Source { get; set; } = "Local DDInter-based database";
}