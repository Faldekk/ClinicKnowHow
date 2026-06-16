using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DrugCompare.Application.Models;
using DrugCompare.Application.Services.Contracts;
using System.Collections.ObjectModel;

namespace DrugCompare.Features.IcdLooker;

public sealed class IcdLookerViewModel : ObservableObject
{
    private readonly IIcdCodeService _icdCodeService;

    private string _icdSearchQuery = string.Empty;
    private string _selectedIcdCategory = "All";
    private IcdCodeItem? _selectedIcdCode;
    private string _statusMessage = "Ready.";
    private bool _isBusy;

    public IcdLookerViewModel(IIcdCodeService icdCodeService)
    {
        _icdCodeService = icdCodeService;

        SearchIcdCodesCommand = new AsyncRelayCommand(SearchIcdCodesAsync);
        LoadIcdCategoriesCommand = new AsyncRelayCommand(LoadIcdCategoriesAsync);
    }

    public string IcdSearchQuery
    {
        get => _icdSearchQuery;
        set => SetProperty(ref _icdSearchQuery, value);
    }

    public string SelectedIcdCategory
    {
        get => _selectedIcdCategory;
        set => SetProperty(ref _selectedIcdCategory, value);
    }

    public IcdCodeItem? SelectedIcdCode
    {
        get => _selectedIcdCode;
        set => SetProperty(ref _selectedIcdCode, value);
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

    public ObservableCollection<IcdCodeItem> IcdSearchResults { get; } = new();

    public ObservableCollection<string> IcdCategories { get; } = new()
    {
        "All"
    };

    public IAsyncRelayCommand SearchIcdCodesCommand { get; }
    public IAsyncRelayCommand LoadIcdCategoriesCommand { get; }

    private async Task SearchIcdCodesAsync()
    {
        IcdSearchResults.Clear();
        SelectedIcdCode = null;

        if (string.IsNullOrWhiteSpace(IcdSearchQuery))
        {
            StatusMessage = "Enter ICD-11 code, disease name, or description.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Searching ICD-11 codes...";

        try
        {
            var categoryFilter = SelectedIcdCategory == "All"
                ? null
                : SelectedIcdCategory;

            var results = await _icdCodeService.SearchAsync(
                IcdSearchQuery,
                categoryFilter,
                100);

            foreach (var item in results)
            {
                IcdSearchResults.Add(item);
            }

            SelectedIcdCode = IcdSearchResults.FirstOrDefault();

            StatusMessage = $"Found {IcdSearchResults.Count} ICD-11 code(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"ICD-11 search failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadIcdCategoriesAsync()
    {
        try
        {
            var current = SelectedIcdCategory;

            IcdCategories.Clear();
            IcdCategories.Add("All");

            var categories = await _icdCodeService.GetCategoriesAsync();

            foreach (var category in categories)
            {
                IcdCategories.Add(category);
            }

            SelectedIcdCategory = IcdCategories.Contains(current)
                ? current
                : "All";

            StatusMessage = "ICD-11 categories loaded.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Loading ICD-11 categories failed: {ex.Message}";
        }
    }
}
