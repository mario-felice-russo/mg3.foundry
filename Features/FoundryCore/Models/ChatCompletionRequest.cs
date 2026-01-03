using System.Text.Json.Serialization;

namespace mg3.foundry.Features.FoundryCore.Models;

public class ChatCompletionRequest
{
    [JsonPropertyName("model")] public string Model { get; set; } = string.Empty;
    [JsonPropertyName("messages")] public List<ChatMessage> Messages { get; set; } = new();
    [JsonPropertyName("temperature")] public float? Temperature { get; set; }
    [JsonPropertyName("top_p")] public float? TopP { get; set; }
    [JsonPropertyName("n")] public int? N { get; set; }
    [JsonPropertyName("stream")] public bool? Stream { get; set; }
    [JsonPropertyName("stop")] public List<string>? Stop { get; set; }
    [JsonPropertyName("max_tokens")] public int? MaxTokens { get; set; }
    [JsonPropertyName("presence_penalty")] public float? PresencePenalty { get; set; }
    [JsonPropertyName("frequency_penalty")] public float? FrequencyPenalty { get; set; }
}
