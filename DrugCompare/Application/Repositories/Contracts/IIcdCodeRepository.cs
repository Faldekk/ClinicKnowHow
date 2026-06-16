using DrugCompare.Application.Models;

namespace DrugCompare.Application.Repositories.Contracts;

public interface IIcdCodeRepository
{
    Task<List<IcdCodeItem>> SearchAsync(
        string query,
        string? chapterFilter,
        int limit = 100);

    Task<List<IcdCodeItem>> SearchCodesAsync(
        string query,
        string? chapterFilter,
        int limit = 100);

    Task<List<string>> GetCategoriesAsync();
}