namespace RClaude.Claude;

public abstract record StreamEvent
{
    public record TextDelta(string Text) : StreamEvent;
    public record ToolUse(string ToolName) : StreamEvent;
    public record ToolResult(string ToolName) : StreamEvent;
    public record Complete(ClaudeResult Result) : StreamEvent;
}
