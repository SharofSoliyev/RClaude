using System.Text.Json;

namespace RClaude.Claude;

public static class StreamJsonParser
{
    /// <summary>
    /// Parse a single stream-json line and return a StreamEvent, or null if not relevant.
    /// </summary>
    public static StreamEvent? ParseLine(string jsonLine)
    {
        var trimmed = jsonLine.Trim();
        if (string.IsNullOrEmpty(trimmed) || !trimmed.StartsWith('{'))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(trimmed);
            var root = doc.RootElement;

            var type = root.TryGetProperty("type", out var typeProp)
                ? typeProp.GetString() : null;

            return type switch
            {
                "assistant" => ParseAssistantLine(root),
                "result" => ParseResultLine(root),
                _ => null
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static StreamEvent? ParseAssistantLine(JsonElement root)
    {
        if (!root.TryGetProperty("message", out var message))
            return null;

        if (!message.TryGetProperty("content", out var content))
            return null;

        if (content.ValueKind != JsonValueKind.Array)
            return null;

        // Return the first meaningful event from this line
        foreach (var block in content.EnumerateArray())
        {
            var blockType = block.TryGetProperty("type", out var bt)
                ? bt.GetString() : null;

            switch (blockType)
            {
                case "text":
                    if (block.TryGetProperty("text", out var text))
                    {
                        var t = text.GetString();
                        if (!string.IsNullOrEmpty(t))
                            return new StreamEvent.TextDelta(t);
                    }
                    break;

                case "tool_use":
                    var toolName = block.TryGetProperty("name", out var name)
                        ? name.GetString() ?? "unknown" : "unknown";
                    return new StreamEvent.ToolUse(toolName);

                case "tool_result":
                    var resultTool = block.TryGetProperty("name", out var rn)
                        ? rn.GetString() ?? "tool" : "tool";
                    return new StreamEvent.ToolResult(resultTool);
            }
        }

        return null;
    }

    private static StreamEvent? ParseResultLine(JsonElement root)
    {
        var result = new ClaudeResult();

        if (root.TryGetProperty("session_id", out var sid))
            result.SessionId = sid.GetString();

        if (root.TryGetProperty("cost_usd", out var cost))
            result.CostUsd = cost.GetDouble();

        if (root.TryGetProperty("duration_ms", out var dur))
            result.DurationMs = dur.GetInt32();

        if (root.TryGetProperty("is_error", out var err))
            result.IsError = err.GetBoolean();

        if (root.TryGetProperty("result", out var resultText)
            && resultText.ValueKind == JsonValueKind.String)
        {
            result.Text = resultText.GetString() ?? "";
        }

        return new StreamEvent.Complete(result);
    }
}
