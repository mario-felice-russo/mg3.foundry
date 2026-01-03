using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mg3.foundry.Features.FoundryCore.Models;
using mg3.foundry.Features.FoundryCore.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace mg3.foundry.Features.Home;

public partial class MainViewModel : ObservableObject
{
    private readonly FoundryServiceApi _foundryService;

    [ObservableProperty]
    private bool _isFoundryInstalled;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Checking Foundry service...";

    [ObservableProperty]
    private ObservableCollection<FoundryModelInfo> _cachedModels = new();

    [ObservableProperty]
    private string _serviceStatusText = "Service Not Running";

    public MainViewModel(FoundryServiceApi foundryService)
    {
        _foundryService = foundryService;
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            var status = await _foundryService.GetStatusAsync();
            IsFoundryInstalled = status != null && status.Endpoints.Any();

            if (IsFoundryInstalled)
            {
                StatusMessage = "Foundry Local service is running and ready";
                await LoadCachedModelsAsync();
            }
            else
            {
                StatusMessage = "Foundry Local service is not running. Please start it first.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            IsFoundryInstalled = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadCachedModelsAsync()
    {
        IsLoading = true;

        try
        {
            var catalog = await _foundryService.GetCatalogAsync();

            var active = await _foundryService.GetActiveModelsAsync();
            Debug.WriteLine("Cached Models");
            active.Value.ForEach(a => Debug.WriteLine(a.Id.Substring(0, a.Id.Length - 2)));

            CachedModels.Clear();

            Debug.WriteLine("All Models");
            foreach (var model in catalog.Value)
            {
                Debug.WriteLine(model.DisplayName);
                if (active.Value.Any(m => m.Id.Substring(0, m.Id.Length - 2) == model.DisplayName))
                {
                    model.IsCached = true;
                    CachedModels.Add(model);
                }
            }

            StatusMessage = $"Found {CachedModels.Count} cached model(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading models: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToChat()
    {
        await Shell.Current.GoToAsync("//ChatPage");
    }

    [RelayCommand]
    private async Task NavigateToModels()
    {
        await Shell.Current.GoToAsync("//ModelsPage");
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadCachedModelsAsync();
    }
}
