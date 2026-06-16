using DrugCompare.Application.Models;

namespace DrugCompare.Application.Services.Contracts;

public interface IInteractionCheckerService
{
    Task<List<InteractionResult>> CheckInteractionsAsync(
        IReadOnlyCollection<ActiveSubstanceItem> substances);
}
