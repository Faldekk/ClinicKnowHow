using DrugCompare.Application.Models;

namespace DrugCompare.Application.Repositories.Contracts;

public interface IDataManagementRepository
{
    Task<DataManagementStatusResult> GetDataManagementStatusAsync();
}
