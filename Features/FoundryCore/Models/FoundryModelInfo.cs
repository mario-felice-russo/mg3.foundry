using System.Text.Json;
using System.Text.Json.Serialization;

namespace mg3.foundry.Features.FoundryCore.Models;

public class FoundryModelInfo
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("displayName")] public string DisplayName { get; set; } = string.Empty;
    [JsonPropertyName("providerType")] public string ProviderType { get; set; } = string.Empty;
    [JsonPropertyName("uri")] public string Uri { get; set; } = string.Empty;
    [JsonPropertyName("version")] public string Version { get; set; } = string.Empty;
    [JsonPropertyName("modelType")] public string ModelType { get; set; } = string.Empty;

    // Matching observed payload: these were empty strings in successful requests
    [JsonPropertyName("architecture")] public string Architecture { get; set; } = string.Empty;
    [JsonPropertyName("fileSize")] public string FileSize { get; set; } = string.Empty;
    [JsonPropertyName("parameterSize")] public string ParameterSize { get; set; } = string.Empty;

    // Matching observed payload: these were null in successful requests
    [JsonPropertyName("path")] public string? Path { get; set; }
    [JsonPropertyName("icon")] public string? Icon { get; set; }
    [JsonPropertyName("fineTuningTemplateName")] public string? FineTuningTemplateName { get; set; }

    // Fields that can be strings or complex objects
    [JsonPropertyName("publisher")] public JsonElement? Publisher { get; set; }
    [JsonPropertyName("runtime")] public JsonElement? Runtime { get; set; }
    [JsonPropertyName("task")] public JsonElement? Task { get; set; }
    [JsonPropertyName("promptTemplate")] public JsonElement? PromptTemplate { get; set; }

    [JsonIgnore] public bool IsCached { get; set; }

    [JsonIgnore] public string Category { get; set; } = "Other";

    [JsonIgnore] public bool IsFavorite { get; set; }
}
