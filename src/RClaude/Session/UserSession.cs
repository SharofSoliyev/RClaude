namespace RClaude.Session;

public class UserSession
{
    public int DbSessionId { get; set; }
    public string SessionName { get; set; } = "Default";
    public long TelegramUserId { get; init; }
    public string? WorkingDirectory { get; set; }
    public string? ClaudeSessionId { get; set; }
    public string Model { get; set; } = "sonnet";
    public bool IsProcessing { get; set; }
    public SemaphoreSlim Lock { get; } = new(1, 1);
}
