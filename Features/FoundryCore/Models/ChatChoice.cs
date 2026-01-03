using System.Text.Json.Serialization;

namespace mg3.foundry.Features.FoundryCore.Models;

public class ChatChoice
{
    [JsonPropertyName("index")] public int Index { get; set; }
    [JsonPropertyName("message")] public ChatMessage Message { get; set; } = new();
    [JsonPropertyName("finish_reason")] public string FinishReason { get; set; } = string.Empty;
}
