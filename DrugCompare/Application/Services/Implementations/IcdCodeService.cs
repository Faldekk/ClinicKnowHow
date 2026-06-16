using DrugCompare.Application.Models;
using DrugCompare.Application.Repositories.Contracts;
using DrugCompare.Application.Services.Contracts;

namespace DrugCompare.Application.Services.Implementations;

public sealed class IcdCodeService : IIcdCodeService
{
    private readonly IIcdCodeRepository _repository;

    public IcdCodeService(IIcdCodeRepository repository)
    {
        _repository = repository;
    }

    public Task<List<IcdCodeItem>> SearchAsync(
        string query,
        string? chapterFilter,
        int limit = 100)
    {
        return _repository.SearchAsync(query, chapterFilter, limit);
    }

    public Task<List<IcdCodeItem>> SearchCodesAsync(
        string query,
        string? chapterFilter,
        int limit = 100)
    {
        return _repository.SearchCodesAsync(query, chapterFilter, limit);
    }

    public Task<List<string>> GetCategoriesAsync()
    {
        return _repository.GetCategoriesAsync();
    }
}