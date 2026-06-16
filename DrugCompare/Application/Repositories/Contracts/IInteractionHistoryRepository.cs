using DrugCompare.Application.Models;

namespace DrugCompare.Application.Repositories.Contracts;

public interface IInteractionHistoryRepository
{
    Task SaveInteractionCheckAsync( IReadOnlyCollection<ActiveSubstanceItem> substances,  IReadOnlyCollection<InteractionResult> results);

    Task<List<InteractionHistoryItem>> GetRecentHistoryAsync(int limit = 20);
}
