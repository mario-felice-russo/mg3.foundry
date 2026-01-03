using System.Text.Json.Serialization;

namespace mg3.foundry.Features.FoundryCore.Models;

public class ChatMessage
{
    [JsonPropertyName("role")] public string Role { get; set; } = string.Empty;

    // Can be string or List<ChatMessageContentPart>
    [JsonPropertyName("content")] public object Content { get; set; } = string.Empty;
}

public class ChatMessageContentPart
{
    [JsonPropertyName("type")] public string Type { get; set; } = "text"; // "text" or "image_url"

    [JsonPropertyName("text")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Text { get; set; }

    [JsonPropertyName("image_url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChatMessageImageUrl? ImageUrl { get; set; }
}

public class ChatMessageImageUrl
{
    [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;
}
