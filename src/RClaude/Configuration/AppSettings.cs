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
    public string PermissionMode { get; set; } = "bypassPermissions";
}

public class AgentSettings
{
    public string DefaultWorkingDirectory { get; set; } = "";
}
