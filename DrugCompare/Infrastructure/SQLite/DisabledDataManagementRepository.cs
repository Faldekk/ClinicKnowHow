using DrugCompare.Application.Models;
using DrugCompare.Application.Repositories.Contracts;
using System.Threading.Tasks;

namespace DrugCompare.Infrastructure.SQLite;

public sealed class DisabledDataManagementRepository : IDataManagementRepository
{
    public Task<DataManagementStatusResult> GetDataManagementStatusAsync()
    {
        return Task.FromResult(new DataManagementStatusResult());
    }
}
