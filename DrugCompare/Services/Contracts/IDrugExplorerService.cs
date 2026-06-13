using DrugCompare.Models;

namespace DrugCompare.Services.Contracts;

public interface IDrugExplorerService
{
    Task<List<DrugExplorerResult>> SearchAsync(string query, int limit = 50);
}