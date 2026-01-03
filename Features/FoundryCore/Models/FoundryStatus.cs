using System.Text.Json.Serialization;

namespace mg3.foundry.Features.FoundryCore.Models;

public class FoundryStatus
{
    [JsonPropertyName("endpoints")] public List<string> Endpoints { get; set; } = new();
    [JsonPropertyName("modelDirPath")] public string ModelDirPath { get; set; } = string.Empty;
    [JsonPropertyName("pipeName")] public string? PipeName { get; set; }
    [JsonPropertyName("isAutoRegistrationResolved")] public bool IsAutoRegistrationResolved { get; set; }
    [JsonPropertyName("autoRegistrationStatus")] public string AutoRegistrationStatus { get; set; } = string.Empty;
}
