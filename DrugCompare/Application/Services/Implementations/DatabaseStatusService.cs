using DrugCompare.Application.Models;
using DrugCompare.Application.Repositories.Contracts;
using DrugCompare.Application.Services.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrugCompare.Application.Services.Implementations;

public sealed class DatabaseStatusService : IDatabaseStatusService
{
    private readonly IDatabaseStatusRepository _databaseStatusRepository;

    public DatabaseStatusService(IDatabaseStatusRepository databaseStatusRepository)
    {
        _databaseStatusRepository = databaseStatusRepository;
    }

    public Task<DatabaseStatusResult> GetDatabaseStatusAsync()
    {
        return _databaseStatusRepository.GetDatabaseStatusAsync();
    }
}
