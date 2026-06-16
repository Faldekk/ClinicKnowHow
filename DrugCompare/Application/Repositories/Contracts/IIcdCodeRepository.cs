using DrugCompare.Application.Models;

namespace DrugCompare.Application.Repositories.Contracts;

public interface IIcdCodeRepository
{
    Task<List<IcdCodeItem>> SearchAsync(
        string query,
        string? categoryFilter = null,
        int limit = 100);

    Task<IcdCodeItem?> GetByIdAsync(long id);
    Task<List<string>> GetCategoriesAsync();
}
