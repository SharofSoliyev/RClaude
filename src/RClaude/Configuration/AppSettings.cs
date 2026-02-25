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

public class OpenAISettings
{
    /// <summary>
    /// OpenAI API key for Whisper (STT) and GPT (prompt optimization)
    /// </summary>
    public string ApiKey { get; set; } = "";

    /// <summary>
    /// Enable audio message processing (default: true if ApiKey is set)
    /// </summary>
    public bool EnableAudioProcessing { get; set; } = true;

    /// <summary>
    /// Maximum audio duration in seconds (default: 60)
    /// </summary>
    public int MaxAudioDurationSeconds { get; set; } = 60;

    /// <summary>
    /// Model for prompt optimization (default: gpt-4o-mini for cost efficiency)
    /// </summary>
    public string PromptOptimizationModel { get; set; } = "gpt-4o-mini";
}
