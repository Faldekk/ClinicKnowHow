using DrugCompare.Models;

namespace DrugCompare.Repositories.Contracts;

public interface IDrugExplorerRepository
{
    Task<List<DrugExplorerResult>> SearchAsync(string query, int limit = 50);
}