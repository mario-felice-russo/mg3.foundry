using System.Text.Json.Serialization;

namespace mg3.foundry.Features.FoundryCore.Models;

internal class V1ModelsResponse
{
    [JsonPropertyName("data")] public List<V1ModelInfo> Data { get; set; } = new();
}
