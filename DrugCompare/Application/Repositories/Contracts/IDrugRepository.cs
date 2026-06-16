using DrugCompare.Application.Models;

namespace DrugCompare.Application.Repositories.Contracts;

public interface IDrugRepository
{
    Task<DrugLookupResult?> FindDrugAsync(string drugName);
}
