using DrugCompare.Application.Models;

namespace DrugCompare.Application.Services.Contracts;

public interface IIcdCodeService
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