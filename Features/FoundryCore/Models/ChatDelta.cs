using System.Text.Json.Serialization;

namespace mg3.foundry.Features.FoundryCore.Models;

public class ChatDelta
{
    [JsonPropertyName("role")] public string? Role { get; set; }
    [JsonPropertyName("content")] public string? Content { get; set; }
}
