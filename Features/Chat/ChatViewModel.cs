using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mg3.foundry.Features.FoundryCore.Models;
using mg3.foundry.Features.FoundryCore.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using static System.Net.WebRequestMethods;

namespace mg3.foundry.Features.Chat;

public partial class ChatViewModel : ObservableObject
{
    private readonly FoundryServiceApi _foundryService;

    [ObservableProperty]
    private ObservableCollection<V1ModelInfo> _availableModels = new();

    [ObservableProperty]
    private V1ModelInfo? _selectedModel;

    [ObservableProperty]
    private string _userInput = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ChatDisplayMessage> _messages = new();

    [ObservableProperty]
    private bool _isSending;

    [ObservableProperty]
    private bool _isLoadingModels;

    [ObservableProperty]
    private byte[]? _attachedImageBytes;

    [ObservableProperty]
    private ImageSource? _attachedImagePreview;

    [ObservableProperty]
    private string? _attachedFileName;

    [ObservableProperty]
    private string? _attachedFileContent;



    [ObservableProperty]
    private int _estimatedTotalTokens;

    [ObservableProperty]
    private Color _tokenCountColor = Colors.Gray;

    public ChatViewModel(FoundryServiceApi foundryService)
    {
        _foundryService = foundryService;
    }

    partial void OnUserInputChanged(string value) => UpdateTokenEstimation();
    partial void OnAttachedFileContentChanged(string? value) => UpdateTokenEstimation();
    partial void OnAttachedImageBytesChanged(byte[]? value) => UpdateTokenEstimation();
    partial void OnSelectedModelChanged(V1ModelInfo? value) => UpdateTokenEstimation();
    partial void OnMessagesChanged(ObservableCollection<ChatDisplayMessage> value)
    {
        UpdateTokenEstimation();
        value.CollectionChanged += (s, e) => UpdateTokenEstimation();
    }

    [ObservableProperty]
    private bool _enableMarkdown = true;

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadModelsAsync();
    }

    [RelayCommand]
    private async Task LoadModelsAsync()
    {
        IsLoadingModels = true;

        try
        {
            var result = await _foundryService.GetActiveModelsAsync();

            if (!result.IsSuccess)
            {
                if (Shell.Current != null)
                    await Shell.Current.DisplayAlert("Error", result.Error!.Details ?? result.Error.Message, "OK");
                return;
            }

            AvailableModels.Clear();

            foreach (var model in result.Value!)
            {
                AvailableModels.Add(model);
            }

            if (AvailableModels.Any())
            {
                SelectedModel = AvailableModels.First();
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
                await Shell.Current.DisplayAlert("Error", $"Failed to load models: {ex.Message}", "OK");
        }
        finally
        {
            IsLoadingModels = false;
        }
    }


    [RelayCommand]
    private async Task ShowModelInfoAsync()
    {
        if (SelectedModel == null) return;

        var info = $"ID: {SelectedModel.Id}\n" +
                   $"Owner: {SelectedModel.OwnedBy}\n" +
                   $"Created: {DateTimeOffset.FromUnixTimeSeconds(SelectedModel.Created).LocalDateTime}\n\n" +
                   $"Max Input Context: {SelectedModel.MaxInputTokens} tokens\n" +
                   $"Max Output Generation: {SelectedModel.MaxOutputTokens} tokens";

        if (Shell.Current != null)
        {
            await Shell.Current.DisplayAlert("Model Capabilities", info, "Close");
        }
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(UserInput) || SelectedModel == null)
            return;

        // Check for token overflow and file attachment
        if (AttachedFileContent != null && EstimatedTotalTokens > SelectedModel.MaxInputTokens && SelectedModel.MaxInputTokens > 0)
        {
            await SendChunkedMessageAsync();
            return;
        }

        try
        {
            IsSending = true;
            var userMessage = UserInput;
            var currentImage = AttachedImageBytes; // Capture current image
            var currentFileContent = AttachedFileContent; // Capture text file content
            var currentFileName = AttachedFileName;

            UserInput = string.Empty;
            RemoveAttachment(); // Clear UI immediately

            // If text file is attached, append it to the message
            if (!string.IsNullOrEmpty(currentFileContent))
            {
                userMessage += $"\n\nFile: {currentFileName}\n```\n{currentFileContent}\n```";
            }

            var displayMessage = EnableMarkdown
                ? Services.MarkdownParser.ParseToFormattedString(userMessage)
                : userMessage;

            Messages.Add(new ChatDisplayMessage
            {
                Text = displayMessage,
                IsUser = true,
                Timestamp = DateTime.Now,
                AttachedImageBytes = currentImage,
                IsMarkdown = EnableMarkdown
            });

            await PerformChatRequestAsync(userMessage, currentImage);
        }
        finally
        {
            IsSending = false;
        }
    }

    private async Task SendChunkedMessageAsync()
    {
        IsSending = true;
        var originalInput = UserInput;
        var fileContent = AttachedFileContent!;
        var fileName = AttachedFileName!;

        UserInput = string.Empty;
        RemoveAttachment();

        // Calculate chunk size (Safe buffer: 1000 tokens for history/prompt)
        // 1 token ~= 4 chars
        var historyTokens = Messages.Sum(m => EstimateTokens(m.Text));
        var maxTokens = SelectedModel!.MaxInputTokens > 0 ? SelectedModel.MaxInputTokens : 4096;
        var availableTokens = maxTokens - historyTokens - 500; // Leave buffer

        if (availableTokens < 500)
        {
            if (Shell.Current != null)
                await Shell.Current.DisplayAlert("Error", "Context is too full to send chunks. Please clear chat.", "OK");
            IsSending = false;
            return;
        }

        var chunkSizeChars = availableTokens * 4;

        // Simple chunking
        var chunks = Enumerable.Range(0, (fileContent.Length + chunkSizeChars - 1) / chunkSizeChars)
            .Select(i => fileContent.Substring(i * chunkSizeChars, Math.Min(chunkSizeChars, fileContent.Length - i * chunkSizeChars)))
            .ToList();

        var totalChunks = chunks.Count;

        try
        {
            // Send chunks
            for (int i = 0; i < totalChunks; i++)
            {
                var isLast = i == totalChunks - 1;
                var chunkPart = chunks[i];
                var prompt = isLast
                    ? $"[Part {i + 1}/{totalChunks} of {fileName}]\n```\n{chunkPart}\n```\n\n{originalInput}"
                    : $"[Part {i + 1}/{totalChunks} of {fileName}]\n```\n{chunkPart}\n```\n\nI am sending a large file in parts. Please acknowledge receipt of Part {i + 1} and wait for the rest.";

                Messages.Add(new ChatDisplayMessage
                {
                    Text = prompt,
                    IsUser = true,
                    Timestamp = DateTime.Now
                });

                await PerformChatRequestAsync(prompt, null, isLast ? 0.4F : 0.1F); // Low temp for acks
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
                await Shell.Current.DisplayAlert("Error", $"Chunking failed: {ex.Message}", "OK");
        }
        finally
        {
            IsSending = false;
        }
    }

    private async Task PerformChatRequestAsync(string textContent, byte[]? imageBytes = null, float temperature = 0.4F)
    {
        try
        {
            // Construct content
            object messageContent;
            if (imageBytes != null)
            {
                var base64 = Convert.ToBase64String(imageBytes);
                messageContent = new List<ChatMessageContentPart>
                {
                    new ChatMessageContentPart { Type = "text", Text = textContent },
                    new ChatMessageContentPart { Type = "image_url", ImageUrl = new ChatMessageImageUrl { Url = $"data:image/jpeg;base64,{base64}" } }
                };
            }
            else
            {
                messageContent = textContent;
            }

            var request = new ChatCompletionRequest
            {
                Model = SelectedModel!.Id,
                Messages = GetConversationHistory(),
                // Note: GetConversationHistory captures the msg we just added
                MaxTokens = SelectedModel.MaxOutputTokens > 0 ? Math.Min(SelectedModel.MaxOutputTokens, 1024) : 512,
                Temperature = temperature
            };

            Console.WriteLine($"Sending request - Model: {request.Model}");

            var stopwatch = Stopwatch.StartNew();
            var result = await _foundryService.CreateChatCompletionAsync(request);
            stopwatch.Stop();

            if (result.IsSuccess && result.Value!.Choices.Any())
            {
                var choice = result.Value.Choices.First();
                string assistantMessage = ExtractContent(choice.Message.Content);
                var usage = result.Value.Usage;

                var displayMessage = EnableMarkdown
                    ? Services.MarkdownParser.ParseToFormattedString(assistantMessage)
                    : assistantMessage;

                Messages.Add(new ChatDisplayMessage
                {
                    Text = displayMessage,
                    IsUser = false,
                    IsTruncated = choice.FinishReason == "length",
                    Timestamp = DateTime.Now,
                    ExecutionTime = stopwatch.Elapsed,
                    TokenUsageDetails = usage != null ? $"{usage.TotalTokens} tokens" : "",
                    IsMarkdown = EnableMarkdown
                });
            }
            else if (!result.IsSuccess)
            {
                Messages.Add(new ChatDisplayMessage
                {
                    Text = $"Error: {result.Error?.Message}",
                    IsUser = false,
                    IsError = true,
                    Timestamp = DateTime.Now
                });
            }
        }
        catch (Exception ex)
        {
            Messages.Add(new ChatDisplayMessage
            {
                Text = $"Exception: {ex.Message}",
                IsUser = false,
                IsError = true,
                Timestamp = DateTime.Now
            });
        }
    }



    [RelayCommand]
    private async Task ContinueMessageAsync(ChatDisplayMessage message)
    {
        if (message == null || SelectedModel == null) return;

        IsSending = true;

        try
        {
            var request = new ChatCompletionRequest
            {
                Model = SelectedModel.Id,
                Messages = GetConversationHistory(message),
                MaxTokens = SelectedModel.MaxOutputTokens > 0 ? Math.Min(SelectedModel.MaxOutputTokens, 512) : 512,
                Temperature = 0.4F
            };

            // Add continue instruction
            request.Messages.Add(new ChatMessage { Role = "user", Content = "Please continue your previous response." });

            var stopwatch = Stopwatch.StartNew();
            var result = await _foundryService.CreateChatCompletionAsync(request);
            stopwatch.Stop();

            if (result.IsSuccess && result.Value!.Choices.Any())
            {
                var choice = result.Value.Choices.First();
                var newContent = choice.Message.Content;
                var usage = result.Value.Usage;

                // Append new content
                message.Text += newContent;

                // Check if still truncated
                message.IsTruncated = choice.FinishReason == "length";

                // Update metrics
                message.ExecutionTime += stopwatch.Elapsed;
                message.TokenUsageDetails = usage != null ? $"{usage.TotalTokens} tokens ({usage.PromptTokens} prompt, {usage.CompletionTokens} completion)" : "Usage info unavailable";
            }
            else if (!result.IsSuccess)
            {
                if (Shell.Current != null)
                    await Shell.Current.DisplayAlert("Error", result.Error!.Message, "OK");
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
                await Shell.Current.DisplayAlert("Error", $"Continue failed: {ex.Message}", "OK");
        }
        finally
        {
            IsSending = false;
        }
    }

    [RelayCommand]
    private void ClearChat()
    {
        Messages.Clear();
    }

    [RelayCommand]
    private async Task PickAttachmentAsync()
    {
        try
        {
            var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".jpg", ".jpeg", ".png", ".txt", ".cs", ".json", ".xml", ".md", ".py", ".js", ".html", ".css", ".cpp", ".h" } },
                    { DevicePlatform.Android, new[] { "image/*", "text/*" } },
                    { DevicePlatform.iOS, new[] { "public.image", "public.text" } },
                    { DevicePlatform.MacCatalyst, new[] { "public.image", "public.text" } }
                });

            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select an image or code file",
                FileTypes = customFileType
            });

            if (result != null)
            {
                if (result.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    result.FileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                    result.FileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    // Handle Image
                    using var stream = await result.OpenReadAsync();
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    AttachedImageBytes = memoryStream.ToArray();
                    AttachedImagePreview = ImageSource.FromStream(() => new MemoryStream(AttachedImageBytes));
                    AttachedFileName = result.FileName;
                    AttachedFileContent = null;
                }
                else
                {
                    // Handle Text/Code
                    using var stream = await result.OpenReadAsync();
                    using var reader = new StreamReader(stream);
                    AttachedFileContent = await reader.ReadToEndAsync();
                    AttachedFileName = result.FileName;
                    AttachedImageBytes = null;
                    AttachedImagePreview = null;
                }
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
                await Shell.Current.DisplayAlert("Error", $"Attachment failed: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private void RemoveAttachment()
    {
        AttachedImageBytes = null;
        AttachedImagePreview = null;
        AttachedFileName = null;
        AttachedFileContent = null;
    }

    private string ExtractContent(object? content)
    {
        if (content == null) return string.Empty;
        if (content is System.Text.Json.JsonElement element)
        {
            return element.ValueKind == System.Text.Json.JsonValueKind.String
                ? element.GetString() ?? ""
                : element.ToString();
        }
        return content.ToString() ?? "";
    }

    private void UpdateTokenEstimation()
    {
        if (SelectedModel == null) return;

        int count = 0;

        // 1. History (Rough estimate)
        foreach (var msg in Messages)
        {
            count += EstimateTokens(msg.Text);
            // Image token estimation is complex, assuming fixed cost for simplicity if needed, 
            // but currently ignoring exact vision tokens for this rough estimator.
        }

        // 2. Current Input
        count += EstimateTokens(UserInput);

        // 3. Attachment
        if (!string.IsNullOrEmpty(AttachedFileContent))
        {
            count += EstimateTokens(AttachedFileContent);
        }
        // Vision cost (approximate, e.g. 1000 tokens for High res, 85 for Low)
        if (AttachedImageBytes != null)
        {
            count += 1000;
        }

        // System prompt estimate
        count += 20;

        EstimatedTotalTokens = count;

        if (SelectedModel.MaxInputTokens > 0)
        {
            TokenCountColor = EstimatedTotalTokens > SelectedModel.MaxInputTokens ? Colors.Red : Colors.Gray;
        }
    }

    private int EstimateTokens(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        // Standard heuristic: 1 token ~= 4 chars typically, but for code it can be different.
        // Using average 4 chars per token.
        return text.Length / 4;
    }

    private List<ChatMessage> GetConversationHistory(ChatDisplayMessage? inclusiveLimit = null)
    {
        var history = new List<ChatMessage>();

        // Add System Prompt to stabilize behavior
        history.Add(new ChatMessage
        {
            Role = "system",
            Content = "You are a helpful AI assistant. Please respond in the same language as the user."
        });

        foreach (var msg in Messages)
        {
            if (msg.IsError) continue;

            object content;
            if (msg.AttachedImageBytes != null)
            {
                var base64 = Convert.ToBase64String(msg.AttachedImageBytes);
                content = new List<ChatMessageContentPart>
                 {
                     new ChatMessageContentPart { Type = "text", Text = msg.Text },
                     new ChatMessageContentPart { Type = "image_url", ImageUrl = new ChatMessageImageUrl { Url = $"data:image/jpeg;base64,{base64}" } }
                 };
            }
            else
            {
                content = msg.Text;
            }

            history.Add(new ChatMessage
            {
                Role = msg.IsUser ? "user" : "assistant",
                Content = content
            });

            if (inclusiveLimit != null && msg == inclusiveLimit)
                break;
        }
        return history;
    }
}

public partial class ChatDisplayMessage : ObservableObject
{
    [ObservableProperty]
    private string _text = string.Empty;

    [ObservableProperty]
    private bool _isUser;

    [ObservableProperty]
    private bool _isError;

    [ObservableProperty]
    private bool _isTruncated;

    [ObservableProperty]
    private DateTime _timestamp;

    [ObservableProperty]
    private TimeSpan _executionTime;

    [ObservableProperty]
    private string _tokenUsageDetails = string.Empty;

    [ObservableProperty]
    private byte[]? _attachedImageBytes;

    [ObservableProperty]
    private bool _isMarkdown;

    public ImageSource? AttachedImageSource => AttachedImageBytes != null
        ? ImageSource.FromStream(() => new MemoryStream(AttachedImageBytes))
        : null;
}
