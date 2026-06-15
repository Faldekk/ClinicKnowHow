using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DrugCompare.Models;
using DrugCompare.Services.Contracts;
using System.Collections.ObjectModel;

namespace DrugCompare.ViewModels.DrugExplorer;

public sealed class DrugExplorerViewModel : ObservableObject
{
    private readonly IDrugExplorerService _drugExplorerService;
    private readonly IAuditLogService _auditLogService;

    private string _drugExplorerQuery = string.Empty;
    private DrugExplorerResult? _selectedDrugExplorerResult;
    private string _statusMessage = "Ready.";
    private bool _isBusy;

    public DrugExplorerViewModel(
        IDrugExplorerService drugExplorerService,
        IAuditLogService auditLogService)
    {
        _drugExplorerService = drugExplorerService;
        _auditLogService = auditLogService;

        SearchDrugExplorerCommand = new AsyncRelayCommand(SearchDrugExplorerAsync);
    }

    public string DrugExplorerQuery
    {
        get => _drugExplorerQuery;
        set => SetProperty(ref _drugExplorerQuery, value);
    }

    public DrugExplorerResult? SelectedDrugExplorerResult
    {
        get => _selectedDrugExplorerResult;
        set => SetProperty(ref _selectedDrugExplorerResult, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public ObservableCollection<DrugExplorerResult> DrugExplorerResults { get; } = new();

    public IAsyncRelayCommand SearchDrugExplorerCommand { get; }

    private async Task SearchDrugExplorerAsync()
    {
        DrugExplorerResults.Clear();
        SelectedDrugExplorerResult = null;

        if (string.IsNullOrWhiteSpace(DrugExplorerQuery))
        {
            StatusMessage = "Enter drug name to search.";
            return;
        }

        var query = DrugExplorerQuery.Trim();

        IsBusy = true;
        StatusMessage = "Searching drug database...";

        try
        {
            var results = await _drugExplorerService.SearchAsync(query, 50);

            foreach (var result in results)
            {
                DrugExplorerResults.Add(result);
            }

            SelectedDrugExplorerResult = DrugExplorerResults.FirstOrDefault();

            StatusMessage = $"Found {DrugExplorerResults.Count} drug record(s).";

            await SafeAuditAsync("DrugExplorerSearched", new
            {
                Query = query,
                ResultCount = DrugExplorerResults.Count,
                Timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Drug explorer search failed: {ex.Message}";

            await SafeAuditAsync("DrugExplorerSearchFailed", new
            {
                Query = query,
                Error = ex.Message,
                Timestamp = DateTime.Now
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SafeAuditAsync(string eventType, object details)
    {
        try
        {
            await _auditLogService.WriteAsync(eventType, details);
        }
        catch
        {
            // Audit log is non-critical in the current version.
        }
    }
}