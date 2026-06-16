using DrugCompare.Application.Models;

namespace DrugCompare.Application.Repositories.Contracts;

public interface IInteractionRepository
{
    Task<List<InteractionResult>> CheckInteractionsAsync(
        IReadOnlyCollection<ActiveSubstanceItem> substances);
}
