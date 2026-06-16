using DrugCompare.Application.Models;
using DrugCompare.Application.Services.Contracts;

namespace DrugCompare.Application.Services.Implementations;

public sealed class DisabledDataManagementService : IDataManagementService
{
    public Task<DataManagementStatusResult> GetDataManagementStatusAsync()
    {
        return Task.FromResult(new DataManagementStatusResult());
    }
}
