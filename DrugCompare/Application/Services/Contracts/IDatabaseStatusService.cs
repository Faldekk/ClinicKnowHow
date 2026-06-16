using DrugCompare.Application.Models;

namespace DrugCompare.Application.Services.Contracts;

public interface IDatabaseStatusService
{
    Task<DatabaseStatusResult> GetDatabaseStatusAsync();
}
