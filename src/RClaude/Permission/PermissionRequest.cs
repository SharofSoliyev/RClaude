using System.Text.Json.Serialization;

namespace RClaude.Permission;

public class PermissionRequest
{
    [JsonPropertyName("userId")]
    public long UserId { get; set; }

    [JsonPropertyName("chatId")]
    public long ChatId { get; set; }

    [JsonPropertyName("toolName")]
    public string ToolName { get; set; } = "";

    [JsonPropertyName("toolInput")]
    public object? ToolInput { get; set; }

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = "";
}

public class PermissionResponse
{
    [JsonPropertyName("decision")]
    public string Decision { get; set; } = "deny";
}
