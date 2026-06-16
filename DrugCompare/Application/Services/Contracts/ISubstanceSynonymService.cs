using DrugCompare.Application.Models;

namespace DrugCompare.Application.Services.Contracts;

public interface ISubstanceSynonymService
{
    Task AddSynonymAsync(
        long activeSubstanceId,
        string synonym,
        string source = "manual");

    Task<List<ActiveSubstanceSynonymItem>> GetSynonymsAsync(long activeSubstanceId);
}
