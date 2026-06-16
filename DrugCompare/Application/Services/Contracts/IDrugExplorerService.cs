using DrugCompare.Application.Models;

namespace DrugCompare.Application.Services.Contracts;

public interface IDrugExplorerService
{
    Task<List<DrugExplorerResult>> SearchAsync(string query, int limit = 50);
}
