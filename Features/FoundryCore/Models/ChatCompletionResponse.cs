using System.Text.Json.Serialization;

namespace mg3.foundry.Features.FoundryCore.Models;

public class ChatCompletionResponse
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("object")] public string Object { get; set; } = "chat.completion";
    [JsonPropertyName("created")] public long Created { get; set; }
    [JsonPropertyName("model")] public string Model { get; set; } = string.Empty;
    [JsonPropertyName("choices")] public List<ChatChoice> Choices { get; set; } = new();
    [JsonPropertyName("usage")] public ChatUsage? Usage { get; set; }
}
