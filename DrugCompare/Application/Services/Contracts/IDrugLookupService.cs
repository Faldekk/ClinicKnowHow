using DrugCompare.Application.Models;

namespace DrugCompare.Application.Services.Contracts;

public interface IDrugLookupService
{
    Task<DrugLookupResult?> FindDrugAsync(string drugName);
}
