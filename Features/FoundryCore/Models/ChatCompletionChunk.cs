using System.Text.Json.Serialization;

namespace mg3.foundry.Features.FoundryCore.Models;

public class ChatCompletionChunk
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("object")] public string Object { get; set; } = "chat.completion.chunk";
    [JsonPropertyName("created")] public long Created { get; set; }
    [JsonPropertyName("model")] public string Model { get; set; } = string.Empty;
    [JsonPropertyName("choices")] public List<ChatChunkChoice> Choices { get; set; } = new();
}
