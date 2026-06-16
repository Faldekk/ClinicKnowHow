using DrugCompare.Application.Models;

namespace DrugCompare.Application.Services.Contracts;

public interface IIcdCodeService
{
    Task<List<IcdCodeItem>> SearchAsync(
        string query,
        string? categoryFilter = null,
        int limit = 100);

    Task<IcdCodeItem?> GetByIdAsync(long id);
    Task<List<string>> GetCategoriesAsync();
}
