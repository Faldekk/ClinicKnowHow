using DrugCompare.Application.Models;

namespace DrugCompare.Application.Services.Contracts;

public interface ISubstanceLookupService
{
    Task<ActiveSubstanceItem?> FindActiveSubstanceAsync(string substanceName);
}
