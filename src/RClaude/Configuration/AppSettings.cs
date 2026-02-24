namespace RClaude.Configuration;

public class TelegramSettings
{
    public string BotToken { get; set; } = "";
    public long[] AllowedUserIds { get; set; } = [];
    public string[] AllowedUsernames { get; set; } = [];
}

public class ClaudeSettings
{
    public string CliBinaryPath { get; set; } = "claude";
    public string Model { get; set; } = "sonnet";
    public int MaxTimeoutSeconds { get; set; } = 600;

    /// <summary>
    /// "full" — barcha toollar avtomatik ruxsat (xuddi eski --dangerously-skip-permissions)
    /// "ask"  — xavfli toollar uchun Telegram button orqali so'raydi
    /// </summary>
    public string PermissionMode { get; set; } = "ask";
}

public class AgentSettings
{
    public string DefaultWorkingDirectory { get; set; } = "";
}
