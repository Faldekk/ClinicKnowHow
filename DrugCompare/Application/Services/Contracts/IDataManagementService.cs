using DrugCompare.Application.Models;

namespace DrugCompare.Application.Services.Contracts;

public interface IDataManagementService
{
    Task<DataManagementStatusResult> GetDataManagementStatusAsync();
}
