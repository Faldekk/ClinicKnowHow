using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DrugCompare.Application.Models;
using DrugCompare.Application.Services.Contracts;

namespace DrugCompare.Features.IcdLooker;

public sealed partial class IcdLookerViewModel : ObservableObject
{
    private readonly IIcdCodeService _icdCodeService;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string? selectedCategory;

    [ObservableProperty]
    private IcdCodeItem? selectedResult;

    [ObservableProperty]
    private string statusMessage = "Ready.";

    [ObservableProperty]
    private bool isBusy;

    public ObservableCollection<IcdCodeItem> Results { get; } = new();

    public ObservableCollection<string> Categories { get; } = new();

    public IcdLookerViewModel(IIcdCodeService icdCodeService)
    {
        _icdCodeService = icdCodeService;
        _ = LoadCategoriesAsync();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Searching ICD codes...";

            Results.Clear();
            SelectedResult = null;

            var chapterFilter =
                string.IsNullOrWhiteSpace(SelectedCategory) ||
                SelectedCategory.Equals("All", StringComparison.OrdinalIgnoreCase)
                    ? null
                    : SelectedCategory;

            var items = await _icdCodeService.SearchCodesAsync(
                SearchText,
                chapterFilter,
                limit: 100);

            foreach (var item in items)
            {
                Results.Add(item);
            }

            SelectedResult = Results.FirstOrDefault();

            StatusMessage = $"Found {Results.Count} ICD code(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"ICD search failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        try
        {
            Categories.Clear();
            Categories.Add("All");

            var categories = await _icdCodeService.GetCategoriesAsync();

            foreach (var category in categories)
            {
                if (!string.IsNullOrWhiteSpace(category))
                {
                    Categories.Add(category);
                }
            }

            SelectedCategory = "All";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Loading ICD chapters failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Clear()
    {
        SearchText = string.Empty;
        SelectedCategory = "All";
        Results.Clear();
        SelectedResult = null;
        StatusMessage = "Cleared.";
    }
}