using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Azure;
using Azure.AI.OpenAI;
using mg3.foundry.Features.FoundryCore.Models;

namespace mg3.foundry.Features.FoundryCore.Services;

/// <summary>
/// Service to interact with the Microsoft Foundry Local REST API with dynamic discovery.
/// </summary>
public class FoundryServiceApi
{
    private HttpClient? _httpClient;
    private OpenAIClient? _openAIClient;
    private string? _discoveredBaseUrl;
    private readonly string _defaultBaseUrl;

    public FoundryServiceApi(string defaultBaseUrl = "http://localhost:55267")
    {
        _defaultBaseUrl = defaultBaseUrl.TrimEnd('/');
    }

    private async Task EnsureInitializedAsync()
    {
        if (_httpClient != null && _discoveredBaseUrl != null) return;

        _discoveredBaseUrl = await DiscoverBaseUrlAsync();

        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(_discoveredBaseUrl);
        _httpClient.Timeout = TimeSpan.FromMinutes(10); // Allow long-running operations (chat, downloads)
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Initialize OpenAIClient pointing to the /openai endpoint as suggested
        _openAIClient = new OpenAIClient(
            new Uri($"{_discoveredBaseUrl}/openai"),
            new AzureKeyCredential("not-needed-for-local")
        );
    }

    private async Task<string> DiscoverBaseUrlAsync()
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "foundry",
                Arguments = "service status",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

#if WINDOWS
            using var process = Process.Start(processInfo);
            if (process == null) return _defaultBaseUrl;

            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            // Match http://127.0.0.1:XXXXX or http://localhost:XXXXX
            var match = Regex.Match(output, @"http://(127\.0\.0\.1|localhost):\d+");
            if (match.Success)
            {
                return match.Value;
            }
#else
            await Task.CompletedTask;
            return _defaultBaseUrl;
#endif
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Discovery failed: {ex.Message}");
        }
        return _defaultBaseUrl;
    }

    #region Model Management

    public async Task<Result<List<FoundryModelInfo>>> GetCatalogAsync()
    {
        await EnsureInitializedAsync();
        var result = await HttpHelper.GetAsync<List<FoundryModelInfo>>(_httpClient!, "foundry/list");

        if (result.IsSuccess && (result.Value == null || result.Value.Count == 0))
        {
            Debug.WriteLine("GetCatalogAsync: No models found, returning empty list");
            return Result<List<FoundryModelInfo>>.Success(new List<FoundryModelInfo>());
        }

        return result;
    }

    public async Task<Result<List<V1ModelInfo>>> GetActiveModelsAsync()
    {
        await EnsureInitializedAsync();
        var result = await HttpHelper.GetAsync<V1ModelsResponse>(_httpClient!, "v1/models");

        if (!result.IsSuccess)
            return Result<List<V1ModelInfo>>.Failure(result.Error!.Message, result.Error.Details, result.Error.StatusCode);

        return Result<List<V1ModelInfo>>.Success(result.Value?.Data ?? new List<V1ModelInfo>());
    }

    public async Task<bool> DownloadModelAsync(FoundryModelInfo model, IProgress<double>? progress = null)
    {
        try
        {
            await EnsureInitializedAsync();

            // CLONE and SANITIZE: 
            // 1. Local service expects "AzureFoundryLocal"
            // 2. The server fails with 500 if 'runtime' is sent as an object (as the catalog provides it),
            //    expecting a string or null instead.
            var modelToDownload = new FoundryModelInfo
            {
                Name = model.Name,
                DisplayName = model.DisplayName,
                ProviderType = model.ProviderType == "AzureFoundry" ? "AzureFoundryLocal" : model.ProviderType,
                Uri = model.Uri,
                Version = model.Version,
                ModelType = model.ModelType,
                Architecture = model.Architecture,
                FileSize = model.FileSize,
                ParameterSize = model.ParameterSize,
                Path = model.Path,
                Icon = model.Icon,
                FineTuningTemplateName = model.FineTuningTemplateName,
                Publisher = model.Publisher,
                Task = model.Task,
                PromptTemplate = model.PromptTemplate,
                // Nullify runtime if it's an object to avoid "Cannot convert StartObject to String" error
                Runtime = (model.Runtime.HasValue && model.Runtime.Value.ValueKind == JsonValueKind.Object) ? null : model.Runtime
            };

            var request = new DownloadModelRequest { Model = modelToDownload };

            // Debug: log the exact payload we are sending
            var json = JsonSerializer.Serialize(request);
            Debug.WriteLine($"Download Request JSON: {json}");

            // Use SendAsync with ResponseHeadersRead because this is a long-running streaming response
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "openai/download");
            httpRequest.Content = JsonContent.Create(request);

            using var response = await _httpClient!.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Download ERROR Response (Status {response.StatusCode}): {error}");
                return false;
            }

            Debug.WriteLine("Download started (Streaming response headers received)");

            // Read the stream to completion to ensure the download finishes on the server
            // and to eventually capture progress
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    Debug.WriteLine($"Download Progress Raw: {line}");

                    // Try to parse progress if it's a JSON line like: {"progress": 45}
                    // Or if it's just a percentage string like "45%"
                    try
                    {
                        if (line.Contains("%"))
                        {
                            var match = Regex.Match(line, @"(\d+)%");
                            if (match.Success && double.TryParse(match.Groups[1].Value, out var p))
                            {
                                progress?.Report(p / 100.0);
                            }
                        }
                        else if (line.Trim().StartsWith("{"))
                        {
                            using var doc = JsonDocument.Parse(line);
                            if (doc.RootElement.TryGetProperty("percentage", out var pProp) ||
                                doc.RootElement.TryGetProperty("progress", out pProp))
                            {
                                if (pProp.ValueKind == JsonValueKind.Number)
                                {
                                    progress?.Report(pProp.GetDouble() / 100.0);
                                }
                            }
                        }
                    }
                    catch { /* ignore parsing errors */ }
                }
            }

            Debug.WriteLine("Download completed (Stream ended)");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error downloading model {model.Name}: {ex.Message}");
            return false;
        }
    }

    public async Task<Result<bool>> DeleteModelAsync(string modelId)
    {
        await EnsureInitializedAsync();

        // Encoding dell'ID: trasforma ":" in "%3A" altrimenti l'URL è invalido
        var encodedId = Uri.EscapeDataString(modelId);

        return await HttpHelper.DeleteAsync(_httpClient!, $"v1/models/{encodedId}");
    }

    #endregion

    #region Inference

    public async Task<Result<ChatCompletionResponse>> CreateChatCompletionAsync(ChatCompletionRequest request)
    {
        await EnsureInitializedAsync();
        return await HttpHelper.PostAsync<ChatCompletionRequest, ChatCompletionResponse>(
            _httpClient!,
            "v1/chat/completions",
            request
        );
    }

    public async IAsyncEnumerable<ChatCompletionChunk> CreateChatCompletionStreamAsync(ChatCompletionRequest request)
    {
        await EnsureInitializedAsync();

        Debug.WriteLine($"Chat Completion Stream Request: Model={request.Model}");

        var streamRequest = new
        {
            model = request.Model,
            messages = request.Messages,
            stream = true
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions");
        httpRequest.Content = JsonContent.Create(streamRequest);

        using var response = await _httpClient!.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Chat Stream ERROR (Status {response.StatusCode}): {error}");
            yield break;
        }

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                continue;

            var data = line.Substring(6); // Remove "data: " prefix
            if (data == "[DONE]")
                break;

            ChatCompletionChunk? chunk = null;
            try
            {
                chunk = JsonSerializer.Deserialize<ChatCompletionChunk>(data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to parse chunk: {ex.Message}");
            }

            if (chunk != null)
                yield return chunk;
        }
    }

    #endregion

    #region Service Info

    public async Task<FoundryStatus?> GetStatusAsync()
    {
        try
        {
            await EnsureInitializedAsync();
            return await _httpClient!.GetFromJsonAsync<FoundryStatus>("openai/status");
        }
        catch
        {
            return null;
        }
    }

    #endregion
}
