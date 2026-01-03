using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mg3.foundry.Features.FoundryCore.Services;
using mg3.foundry.Features.FoundryCore.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace mg3.foundry.Features.ModelManagement;

public partial class ModelsViewModel : ObservableObject
{
    private readonly FoundryServiceApi _foundryService;

    [ObservableProperty]
    private ObservableCollection<FoundryModelInfo> _availableModels = new();

    [ObservableProperty]
    private ObservableCollection<FoundryModelInfo> _cachedModels = new();

    [ObservableProperty]
    private ObservableCollection<FoundryModelInfo> _favoriteModels = new();

    [ObservableProperty]
    private string _selectedCategory = "All";

    [ObservableProperty]
    private ObservableCollection<string> _categories = new()
    {
        "All", "Text Generation", "Vision", "Embedding", "Speech", "Other"
    };

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private double _downloadProgress;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ModelsViewModel(FoundryServiceApi foundryService)
    {
        _foundryService = foundryService;
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadModelsAsync();
    }

    [RelayCommand]
    private async Task LoadModelsAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading models...";

        try
        {
            var availableTask = _foundryService.GetCatalogAsync();
            var activeTask = _foundryService.GetActiveModelsAsync();

            await Task.WhenAll(availableTask, activeTask);

            var availableResult = await availableTask;
            var activeResult = await activeTask;

            if (!availableResult.IsSuccess)
            {
                StatusMessage = $"Error loading catalog: {availableResult.Error!.Message}";
                if (Shell.Current != null)
                    await Shell.Current.DisplayAlert("Error", availableResult.Error.Details ?? availableResult.Error.Message, "OK");
                return;
            }

            if (!activeResult.IsSuccess)
            {
                StatusMessage = $"Error loading active models: {activeResult.Error!.Message}";
                if (Shell.Current != null)
                    await Shell.Current.DisplayAlert("Error", activeResult.Error.Details ?? activeResult.Error.Message, "OK");
                return;
            }

            var available = availableResult.Value!;
            var active = activeResult.Value!;

            AvailableModels.Clear();
            CachedModels.Clear();

            available.ForEach(model =>
            {
                model.IsCached = active.Any(c => c.Id == model.Name);
            });

            var sortedModels = available.OrderByDescending(m => m.IsCached).Distinct().ToList();
            sortedModels.ForEach(model => AvailableModels.Add(model));

            foreach (var model in sortedModels.Where(m => m.IsCached))
            {
                CachedModels.Add(model);
            }

            StatusMessage = $"Found {AvailableModels.Count} available models, {CachedModels.Count} cached";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            if (Shell.Current != null)
                await Shell.Current.DisplayAlert("Error", $"Failed to load models: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DownloadModelAsync(FoundryModelInfo model)
    {
        if (model == null)
            return;

        if (Shell.Current == null) return;

        var confirm = await Shell.Current.DisplayAlert(
            "Download Model",
            $"Do you want to download {model.DisplayName}?",
            "Yes",
            "No");

        if (!confirm)
            return;

        IsLoading = true;
        DownloadProgress = 0;
        StatusMessage = $"Downloading {model.DisplayName}...";

        try
        {
            var progressHandler = new Progress<double>(p =>
            {
                DownloadProgress = p;
                StatusMessage = $"Downloading {model.DisplayName}: {(p * 100):F0}%";
            });

            var success = await _foundryService.DownloadModelAsync(model, progressHandler);

            if (success)
            {
                StatusMessage = $"Successfully downloaded {model.DisplayName}";
                await Shell.Current.DisplayAlert("Success", $"{model.DisplayName} has been downloaded", "OK");
                await LoadModelsAsync();
            }
            else
            {
                StatusMessage = $"Failed to download {model.Name}";
                await Shell.Current.DisplayAlert("Error", $"Failed to download {model.Name}", "OK");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            await Shell.Current.DisplayAlert("Error", $"Download failed: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
            DownloadProgress = 0;
        }
    }

    [RelayCommand]
    private async Task DeleteModelAsync(FoundryModelInfo model)
    {
        if (model == null || Shell.Current == null)
            return;

        var confirm = await Shell.Current.DisplayAlert(
            "Delete Model",
            $"Are you sure you want to delete {model.DisplayName}?",
            "Yes",
            "No");

        if (!confirm)
            return;

        IsLoading = true;
        StatusMessage = $"Deleting {model.Name}...";

        try
        {
            var result = await _foundryService.DeleteModelAsync(model.Name);

            if (result.IsSuccess)
            {
                StatusMessage = $"Successfully deleted {model.DisplayName}";
                await LoadModelsAsync();
            }
            else
            {
                StatusMessage = $"Failed to delete {model.Name}: {result.Error!.Message}";
                await Shell.Current.DisplayAlert("Error", result.Error.Details ?? result.Error.Message, "OK");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            await Shell.Current.DisplayAlert("Error", $"Delete failed: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadModelsAsync();
    }

    private string DetermineModelCategory(FoundryModelInfo model)
    {
        if (string.IsNullOrEmpty(model.ModelType))
            return "Other";

        return model.ModelType.ToLower() switch
        {
            var t when t.Contains("text") || t.Contains("llm") || t.Contains("language") => "Text Generation",
            var t when t.Contains("vision") || t.Contains("image") || t.Contains("multimodal") => "Vision",
            var t when t.Contains("embed") || t.Contains("vector") => "Embedding",
            var t when t.Contains("speech") || t.Contains("audio") || t.Contains("tts") || t.Contains("stt") => "Speech",
            _ => "Other"
        };
    }

    private async Task LoadFavoriteModelsAsync()
    {
        // In una implementazione reale, questo caricherebbe da storage locale o API
        // Per ora simuliamo alcuni preferiti
        FavoriteModels.Clear();

        var favoriteModelIds = new List<string> { "phi-3.5-mini", "llama-3-8b" };

        foreach (var modelId in favoriteModelIds)
        {
            var model = AvailableModels.FirstOrDefault(m => m.Name.Contains(modelId, StringComparison.OrdinalIgnoreCase));
            if (model != null)
            {
                model.IsFavorite = true;
                FavoriteModels.Add(model);
            }
        }
    }

    [RelayCommand]
    private void ToggleFavorite(FoundryModelInfo model)
    {
        model.IsFavorite = !model.IsFavorite;

        if (model.IsFavorite)
        {
            if (!FavoriteModels.Contains(model))
            {
                FavoriteModels.Add(model);
            }
        }
        else
        {
            FavoriteModels.Remove(model);
        }

        // In una implementazione reale, salverebbe lo stato
        // await SaveFavoriteModelsAsync();
    }

    [RelayCommand]
    private void FilterByCategory(string category)
    {
        SelectedCategory = category;
        
        // In una implementazione completa, qui si filtrerebbe la lista
        // Per ora aggiorniamo solo il messaggio
        var filteredCount = SelectedCategory == "All"
            ? AvailableModels.Count
            : AvailableModels.Count(m => m.Category == SelectedCategory);

        StatusMessage = $"Showing {filteredCount} model(s) in {SelectedCategory} category";
    }

    [RelayCommand]
    private async Task LoadModelsWithCategoriesAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading models with categories...";

        try
        {
            var availableTask = _foundryService.GetCatalogAsync();
            var activeTask = _foundryService.GetActiveModelsAsync();

            await Task.WhenAll(availableTask, activeTask);

            var availableResult = await availableTask;
            var activeResult = await activeTask;

            if (!availableResult.IsSuccess)
            {
                StatusMessage = $"Error loading catalog: {availableResult.Error!.Message}";
                if (Shell.Current != null)
                    await Shell.Current.DisplayAlert("Error", availableResult.Error.Details ?? availableResult.Error.Message, "OK");
                return;
            }

            if (!activeResult.IsSuccess)
            {
                StatusMessage = $"Error loading active models: {activeResult.Error!.Message}";
                if (Shell.Current != null)
                    await Shell.Current.DisplayAlert("Error", activeResult.Error.Details ?? activeResult.Error.Message, "OK");
                return;
            }

            var available = availableResult.Value!;
            var active = activeResult.Value!;

            // Categorizza i modelli
            foreach (var model in available)
            {
                model.Category = DetermineModelCategory(model);
            }

            AvailableModels.Clear();
            CachedModels.Clear();

            available.ForEach(model =>
            {
                model.IsCached = active.Any(c => c.Id == model.Name);
            });

            var sortedModels = available.OrderByDescending(m => m.IsCached).Distinct().ToList();
            sortedModels.ForEach(model => AvailableModels.Add(model));

            foreach (var model in sortedModels.Where(m => m.IsCached))
            {
                CachedModels.Add(model);
            }

            // Carica modelli preferiti
            await LoadFavoriteModelsAsync();

            StatusMessage = $"Found {AvailableModels.Count} available models, {CachedModels.Count} cached, {FavoriteModels.Count} favorites";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            if (Shell.Current != null)
                await Shell.Current.DisplayAlert("Error", $"Failed to load models: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
