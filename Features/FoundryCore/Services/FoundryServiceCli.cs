using System.Diagnostics;
using System.Text;
using mg3.foundry.Features.FoundryCore.Models;

namespace mg3.foundry.Features.FoundryCore.Services;

public class FoundryServiceCli
{
    private const string FoundryCommand = "foundry";

    public async Task<bool> IsFoundryInstalledAsync()
    {
        try
        {
            var result = await ExecuteCommandAsync("--version");
            return result.Success;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<CliModelInfo>> GetAvailableModelsAsync()
    {
        var result = await ExecuteCommandAsync("model list");
        if (!result.Success)
            return new List<CliModelInfo>();

        return ParseModelList(result.Output);
    }

    public async Task<List<CliModelInfo>> GetCachedModelsAsync()
    {
        var result = await ExecuteCommandAsync("cache list");
        if (!result.Success)
            return new List<CliModelInfo>();

        return ParseModelList(result.Output);
    }

    public async Task<bool> DownloadModelAsync(string modelName, IProgress<double>? progress = null)
    {
        var result = await ExecuteCommandAsync($"model download {modelName}", progress);
        return result.Success;
    }

    public async Task<bool> DeleteModelAsync(string modelName)
    {
        var result = await ExecuteCommandAsync($"model delete {modelName}");
        return result.Success;
    }

    public async Task<string> RunModelChatAsync(string modelName, string prompt)
    {
        var escapedPrompt = prompt.Replace("\"", "\\\"");
        var result = await ExecuteCommandAsync($"model run {modelName} --prompt \"{escapedPrompt}\"");
        return result.Success ? result.Output : $"Error: {result.Error}";
    }

    public async Task<ProcessResult> StartModelServerAsync(string modelName, int port = 5272)
    {
        return await ExecuteCommandAsync($"model serve {modelName} --port {port}");
    }

    private async Task<ProcessResult> ExecuteCommandAsync(string arguments, IProgress<double>? progress = null)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = FoundryCommand,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        var output = new StringBuilder();
        var error = new StringBuilder();

        try
        {
            using var process = new Process { StartInfo = processInfo };

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    output.AppendLine(e.Data);

                    if (progress != null && e.Data.Contains("%"))
                    {
                        var percentMatch = System.Text.RegularExpressions.Regex.Match(e.Data, @"(\d+)%");
                        if (percentMatch.Success && double.TryParse(percentMatch.Groups[1].Value, out var percent))
                        {
                            progress.Report(percent);
                        }
                    }
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    error.AppendLine(e.Data);
            };

            if (!process.Start())
            {
                return new ProcessResult
                {
                    Success = false,
                    Output = string.Empty,
                    Error = "Failed to start foundry process.",
                    ExitCode = -1
                };
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            var errorOutput = error.ToString();
            var standardOutput = output.ToString();

            return new ProcessResult
            {
                Success = process.ExitCode == 0,
                Output = standardOutput,
                Error = errorOutput,
                ExitCode = process.ExitCode
            };
        }
        catch (Exception ex)
        {
            return new ProcessResult
            {
                Success = false,
                Output = string.Empty,
                Error = $"Error executing foundry command: {ex.Message}",
                ExitCode = -1
            };
        }
    }

    private List<CliModelInfo> ParseModelList(string output)
    {
        bool isCached = false;
        CliModelInfo? currentModel = default;
        CliModelInfo? previousModel = default;
        var models = new List<CliModelInfo>();
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        string alias = string.Empty, device = string.Empty, task = string.Empty, fileSize = string.Empty, license = string.Empty, modelId = string.Empty;

        foreach (var linea in lines)
        {
            string line = linea.Trim();
            if (line.StartsWith("Models")) isCached = true;

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("Alias") || line.StartsWith("Models") || line.StartsWith("---"))
                continue;

            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 1)
            {
                currentModel = new CliModelInfo();

                if (parts.Length == 7)
                {
                    alias = line.Substring(0, 31).Trim();
                    device = line.Substring(31, 11).Trim();
                    task = line.Substring(42, 15).Trim();
                    fileSize = line.Substring(57, 13).Trim();
                    license = line.Substring(70, 13).Trim();
                    modelId = line.Substring(83, line.Length - 83).Trim();
                }
                else if (parts.Length == 6 && previousModel != null)
                {
                    alias = previousModel.Alias;
                    device = line.Substring(0, 11).Trim();
                    task = line.Substring(11, 15).Trim();
                    fileSize = line.Substring(26, 13).Trim();
                    license = line.Substring(39, 13).Trim();
                    modelId = line.Substring(52, line.Length - 52).Trim();
                }

                if (isCached)
                {
                    if (parts.Length >= 3)
                    {
                        currentModel.Alias = parts[1];
                        currentModel.ModelID = parts[2];
                    }
                }
                else
                {
                    currentModel.Alias = string.IsNullOrEmpty(alias) ? (previousModel?.Alias ?? string.Empty) : alias;
                    currentModel.Device = device;
                    currentModel.Task = task;
                    currentModel.FileSize = fileSize;
                    currentModel.License = license;
                    currentModel.ModelID = modelId;
                }

                models.Add(currentModel);
                previousModel = currentModel;
            }
        }

        return models;
    }
}
