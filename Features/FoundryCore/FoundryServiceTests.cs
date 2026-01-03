using mg3.foundry.Features.FoundryCore.Services;

namespace mg3.foundry.Features.FoundryCore;

/// <summary>
/// Classe di esempio per testare manualmente il FoundryService
/// </summary>
public class FoundryServiceTests
{
    public static async Task TestFoundryInstallation()
    {
        var service = new FoundryServiceCli();

        Console.WriteLine("Testing Foundry installation...");
        var isInstalled = await service.IsFoundryInstalledAsync();
        Console.WriteLine($"Foundry installed: {isInstalled}");
    }

    public static async Task TestListModels()
    {
        var service = new FoundryServiceCli();

        Console.WriteLine("\nListing available models...");
        var models = await service.GetAvailableModelsAsync();

        foreach (var model in models)
        {
            Console.WriteLine($"- {model.Alias} ({model.FileSize})");
        }
    }

    public static async Task TestListCachedModels()
    {
        var service = new FoundryServiceCli();

        Console.WriteLine("\nListing cached models...");
        var models = await service.GetCachedModelsAsync();

        foreach (var model in models)
        {
            Console.WriteLine($"- {model.Alias} ({model.FileSize})");
        }
    }

    public static async Task TestDownloadModel(string modelName)
    {
        var service = new FoundryServiceCli();

        Console.WriteLine($"\nDownloading model: {modelName}");

        var progress = new Progress<double>(percent =>
        {
            Console.Write($"\rProgress: {percent:F1}%");
        });

        var success = await service.DownloadModelAsync(modelName, progress);
        Console.WriteLine($"\n\nDownload {(success ? "succeeded" : "failed")}");
    }

    public static async Task TestRunChat(string modelName, string prompt)
    {
        var service = new FoundryServiceCli();

        Console.WriteLine($"\nRunning chat with model: {modelName}");
        Console.WriteLine($"Prompt: {prompt}");
        Console.WriteLine("\nResponse:");

        var response = await service.RunModelChatAsync(modelName, prompt);
        Console.WriteLine(response);
    }
}
