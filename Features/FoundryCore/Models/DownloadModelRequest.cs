using System.Text.Json.Serialization;

namespace mg3.foundry.Features.FoundryCore.Models;

public class DownloadModelRequest
{
    [JsonPropertyName("bufferSize")]
    public int? BufferSize { get; set; }

    [JsonPropertyName("customDirPath")]
    public string? CustomDirPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".foundry", "cache", "models");

    [JsonPropertyName("ignorePipeReport")]
    public bool IgnorePipeReport { get; set; } = true;

    [JsonPropertyName("model")]
    public FoundryModelInfo Model { get; set; } = null!;

    [JsonPropertyName("progressToken")]
    public string? ProgressToken { get; set; }

    [JsonPropertyName("token")]
    public string? Token { get; set; }
}
