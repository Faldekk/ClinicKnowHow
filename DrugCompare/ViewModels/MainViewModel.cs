using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DrugCompare.Models;
using DrugCompare.Services;
using System.Collections.ObjectModel;

namespace DrugCompare.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly IDrugDataService _drugDataService;

    private string _drugNameInput = string.Empty;
    private string _manualSubstanceInput = string.Empty;
    private ActiveSubstanceItem? _selectedDetectedSubstance;
    private ActiveSubstanceItem? _selectedAcceptedSubstance;
    private InteractionResult? _selectedInteraction;
    private string _statusMessage = "Ready.";
    private bool _isBusy;

    public MainViewModel(IDrugDataService drugDataService)
    {
        _drugDataService = drugDataService;

        FindDrugCommand = new AsyncRelayCommand(FindDrugAsync);
        AcceptDetectedSubstanceCommand = new RelayCommand(AcceptDetectedSubstance);
        AcceptAllDetectedSubstancesCommand = new RelayCommand(AcceptAllDetectedSubstances);
        AddManualSubstanceCommand = new AsyncRelayCommand(AddManualSubstanceAsync);
        RemoveAcceptedSubstanceCommand = new RelayCommand(RemoveAcceptedSubstance);
        ClearCaseCommand = new RelayCommand(ClearCase);
        CheckInteractionsCommand = new AsyncRelayCommand(CheckInteractionsAsync);
    }

    public string DrugNameInput
    {
        get => _drugNameInput;
        set => SetProperty(ref _drugNameInput, value);
    }

    public string ManualSubstanceInput
    {
        get => _manualSubstanceInput;
        set => SetProperty(ref _manualSubstanceInput, value);
    }

    public ActiveSubstanceItem? SelectedDetectedSubstance
    {
        get => _selectedDetectedSubstance;
        set => SetProperty(ref _selectedDetectedSubstance, value);
    }

    public ActiveSubstanceItem? SelectedAcceptedSubstance
    {
        get => _selectedAcceptedSubstance;
        set => SetProperty(ref _selectedAcceptedSubstance, value);
    }

    public InteractionResult? SelectedInteraction
    {
        get => _selectedInteraction;
        set => SetProperty(ref _selectedInteraction, value);
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

    public ObservableCollection<ActiveSubstanceItem> DetectedSubstances { get; } = new();

    public ObservableCollection<ActiveSubstanceItem> AcceptedSubstances { get; } = new();

    public ObservableCollection<InteractionResult> InteractionResults { get; } = new();

    public IAsyncRelayCommand FindDrugCommand { get; }

    public IRelayCommand AcceptDetectedSubstanceCommand { get; }

    public IRelayCommand AcceptAllDetectedSubstancesCommand { get; }

    public IAsyncRelayCommand AddManualSubstanceCommand { get; }

    public IRelayCommand RemoveAcceptedSubstanceCommand { get; }

    public IRelayCommand ClearCaseCommand { get; }

    public IAsyncRelayCommand CheckInteractionsCommand { get; }

    private async Task FindDrugAsync()
    {
        DetectedSubstances.Clear();

        if (string.IsNullOrWhiteSpace(DrugNameInput))
        {
            StatusMessage = "Enter drug name.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Searching local drug dictionary...";

        try
        {
            var result = await _drugDataService.FindDrugAsync(DrugNameInput);

            if (result is null)
            {
                StatusMessage = "Drug not found in local dictionary. Add active substance manually.";
                return;
            }

            foreach (var substance in result.ActiveSubstances)
            {
                DetectedSubstances.Add(substance);
            }

            StatusMessage = $"Found {result.ActiveSubstances.Count} active substance(s) for {result.DrugName}.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Drug lookup failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void AcceptDetectedSubstance()
    {
        if (SelectedDetectedSubstance is null)
        {
            StatusMessage = "Select detected active substance first.";
            return;
        }

        AddAcceptedSubstance(SelectedDetectedSubstance);
    }

    private void AcceptAllDetectedSubstances()
    {
        if (DetectedSubstances.Count == 0)
        {
            StatusMessage = "No detected substances to accept.";
            return;
        }

        foreach (var substance in DetectedSubstances)
        {
            AddAcceptedSubstance(substance);
        }

        StatusMessage = "Detected active substances accepted.";
    }

    private async Task AddManualSubstanceAsync()
    {
        if (string.IsNullOrWhiteSpace(ManualSubstanceInput))
        {
            StatusMessage = "Enter active substance name.";
            return;
        }

        IsBusy = true;

        try
        {
            var substance = await _drugDataService.FindActiveSubstanceAsync(ManualSubstanceInput);

            if (substance is null)
            {
                StatusMessage = "Could not add active substance.";
                return;
            }

            AddAcceptedSubstance(substance);
            ManualSubstanceInput = string.Empty;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Adding substance failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void RemoveAcceptedSubstance()
    {
        if (SelectedAcceptedSubstance is null)
        {
            StatusMessage = "Select active substance to remove.";
            return;
        }

        AcceptedSubstances.Remove(SelectedAcceptedSubstance);
        SelectedAcceptedSubstance = null;

        StatusMessage = "Active substance removed.";
    }

    private void ClearCase()
    {
        DrugNameInput = string.Empty;
        ManualSubstanceInput = string.Empty;

        DetectedSubstances.Clear();
        AcceptedSubstances.Clear();
        InteractionResults.Clear();

        SelectedDetectedSubstance = null;
        SelectedAcceptedSubstance = null;
        SelectedInteraction = null;

        StatusMessage = "Case cleared.";
    }

    private async Task CheckInteractionsAsync()
    {
        InteractionResults.Clear();
        SelectedInteraction = null;

        if (AcceptedSubstances.Count < 2)
        {
            StatusMessage = "At least two active substances are required.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Checking substance interactions...";

        try
        {
            var results = await _drugDataService.CheckInteractionsAsync(AcceptedSubstances.ToList());
            StatusMessage = $"Service returned {results.Count} interaction(s).";

            foreach (var result in results)
            {
                InteractionResults.Add(result);
            }

            if (results.Count == 0)
            {
                StatusMessage = "No known interaction was found in the local DDInter-based database. This does not mean the combination is safe.";
                return;
            }

            SelectedInteraction = InteractionResults.FirstOrDefault();

            StatusMessage = $"Found {results.Count} known interaction(s). Clinical verification is required.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Interaction check failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void AddAcceptedSubstance(ActiveSubstanceItem substance)
    {
        var alreadyExists = AcceptedSubstances.Any(x =>
            x.NormalizedName == substance.NormalizedName);

        if (alreadyExists)
        {
            StatusMessage = $"Accepted: {substance.Name}, DatabaseId: {substance.DatabaseId}, Source: {substance.Source}";
            return;
        }

        AcceptedSubstances.Add(substance);

        StatusMessage = $"Accepted: {substance.Name}, DatabaseId: {substance.DatabaseId}, Source: {substance.Source}";
    }
}