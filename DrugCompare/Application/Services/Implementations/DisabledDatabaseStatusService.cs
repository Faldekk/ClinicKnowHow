using DrugCompare.Application.Models;
using DrugCompare.Application.Services.Contracts;

namespace DrugCompare.Application.Services.Implementations;

public sealed class DisabledDatabaseStatusService : IDatabaseStatusService
{
    public Task<DatabaseStatusResult> GetDatabaseStatusAsync()
    {
        return Task.FromResult(new DatabaseStatusResult());
    }
}
