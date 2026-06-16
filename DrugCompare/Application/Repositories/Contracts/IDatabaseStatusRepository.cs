using DrugCompare.Application.Models;

namespace DrugCompare.Application.Repositories.Contracts;

public interface IDatabaseStatusRepository
{
    Task<DatabaseStatusResult> GetDatabaseStatusAsync();
}
