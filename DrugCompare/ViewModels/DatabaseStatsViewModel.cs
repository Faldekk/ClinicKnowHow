using CommunityToolkit.Mvvm.ComponentModel;
using DrugCompare.Application.Models;

namespace DrugCompare.ViewModels;

public sealed class DatabaseStatusViewModel : ObservableObject
{
    public DatabaseStatusViewModel(DatabaseStatusResult status)
    {
        DrugsCount = status.DrugsCount;
        ActiveSubstancesCount = status.ActiveSubstancesCount;
        DrugActiveSubstancesCount = status.DrugActiveSubstancesCount;
        SubstanceInteractionsCount = status.SubstanceInteractionsCount;
    }

    public long DrugsCount { get; }

    public long ActiveSubstancesCount { get; }

    public long DrugActiveSubstancesCount { get; }

    public long SubstanceInteractionsCount { get; }

    public string Summary =>
        $"Drugs: {DrugsCount:N0} | Active substances: {ActiveSubstancesCount:N0} | Relations: {DrugActiveSubstancesCount:N0} | Interactions: {SubstanceInteractionsCount:N0}";
}
