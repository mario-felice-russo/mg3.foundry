using System.Text.Json.Serialization;

namespace mg3.foundry.Features.FoundryCore.Models;

public class V1ModelInfo
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("maxInputTokens")] public int MaxInputTokens { get; set; }
    [JsonPropertyName("maxOutputTokens")] public int MaxOutputTokens { get; set; }
    [JsonPropertyName("object")] public string Object { get; set; } = "model";
    [JsonPropertyName("created")] public long Created { get; set; }
    [JsonPropertyName("owned_by")] public string OwnedBy { get; set; } = "foundry";
}
