using DrugCompare.Models;

namespace DrugCompare.Services;

public interface IDrugDataService
{
    Task<DrugLookupResult?> FindDrugAsync(string drugName);

    Task<ActiveSubstanceItem?> FindActiveSubstanceAsync(string substanceName);

    Task<List<InteractionResult>> CheckInteractionsAsync(
        IReadOnlyCollection<ActiveSubstanceItem> substances);
}