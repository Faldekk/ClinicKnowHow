using DrugCompare.Application.Models;

namespace DrugCompare.Application.Repositories.Contracts;

public interface IDrugExplorerRepository
{
    Task<List<DrugExplorerResult>> SearchAsync(string query, int limit = 50);
}
