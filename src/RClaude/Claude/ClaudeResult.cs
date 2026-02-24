namespace RClaude.Claude;

public class ClaudeResult
{
    public string Text { get; set; } = "";
    public string? SessionId { get; set; }
    public double? CostUsd { get; set; }
    public int? DurationMs { get; set; }
    public bool IsError { get; set; }
    public List<string> ToolCalls { get; set; } = [];
}
