namespace RClaude.Data.Entities;

public class SessionEntity
{
    public int Id { get; set; }
    public long TelegramUserId { get; set; }
    public string Name { get; set; } = "";
    public string? WorkingDirectory { get; set; }
    public string? ClaudeSessionId { get; set; }
    public string Model { get; set; } = "sonnet";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
