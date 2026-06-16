using DrugCompare.Application.Models;

namespace DrugCompare.Application.Services.Contracts;

public interface IInteractionHistoryService
{
    Task SaveInteractionCheckAsync(
        IReadOnlyCollection<ActiveSubstanceItem> substances,
        IReadOnlyCollection<InteractionResult> results);

    Task<List<InteractionHistoryItem>> GetRecentHistoryAsync(int limit = 20);
}
