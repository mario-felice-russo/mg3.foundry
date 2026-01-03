using System.Text.Json.Serialization;

namespace mg3.foundry.Features.FoundryCore.Models;

public class ChatChunkChoice
{
    [JsonPropertyName("index")] public int Index { get; set; }
    [JsonPropertyName("delta")] public ChatDelta Delta { get; set; } = new();
    [JsonPropertyName("finish_reason")] public string? FinishReason { get; set; }
}
