using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Options;
using RClaude.Configuration;
using RClaude.Session;

namespace RClaude.Claude;

public class ClaudeCliService
{
    private readonly ClaudeSettings _claudeSettings;
    private readonly ILogger<ClaudeCliService> _logger;

    public ClaudeCliService(
        IOptions<ClaudeSettings> claudeSettings,
        ILogger<ClaudeCliService> logger)
    {
        _claudeSettings = claudeSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Send a message to Claude CLI with real-time streaming events.
    /// onEvent is called for each stream event (text delta, tool use, result).
    /// </summary>
    public async Task<ClaudeResult> SendMessageAsync(
        string message,
        UserSession session,
        Func<StreamEvent, Task>? onEvent = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(session.WorkingDirectory))
        {
            return new ClaudeResult
            {
                Text = "Working directory belgilanmagan. /setdir buyrug'ini ishlating.",
                IsError = true
            };
        }

        var args = BuildArguments(message, session);
        _logger.LogInformation("Claude CLI: {Args}", args);

        var psi = new ProcessStartInfo
        {
            FileName = _claudeSettings.CliBinaryPath,
            Arguments = args,
            WorkingDirectory = session.WorkingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        // Remove CLAUDECODE env var to prevent "nested session" error
        psi.Environment.Remove("CLAUDECODE");
        psi.Environment.Remove("CLAUDE_CODE_ENTRYPOINT");

        using var process = new Process { StartInfo = psi };
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(_claudeSettings.MaxTimeoutSeconds));

        var accumulatedText = new StringBuilder();
        var toolCalls = new List<string>();
        ClaudeResult? finalResult = null;

        try
        {
            process.Start();
            process.StandardInput.Close(); // stdin yopamiz — CLI hang bo'lmasligi uchun

            var errorTask = process.StandardError.ReadToEndAsync(cts.Token);

            // Read stream line by line and emit events
            while (!cts.Token.IsCancellationRequested)
            {
                var line = await process.StandardOutput.ReadLineAsync(cts.Token);
                if (line == null) break;

                var streamEvent = StreamJsonParser.ParseLine(line);
                if (streamEvent == null) continue;

                switch (streamEvent)
                {
                    case StreamEvent.TextDelta delta:
                        accumulatedText.Append(delta.Text);
                        break;

                    case StreamEvent.ToolUse tool:
                        toolCalls.Add(tool.ToolName);
                        break;

                    case StreamEvent.Complete complete:
                        finalResult = complete.Result;
                        break;
                }

                // Emit event to caller
                if (onEvent != null)
                {
                    try { await onEvent(streamEvent); }
                    catch { }
                }
            }

            var error = await errorTask;
            await process.WaitForExitAsync(cts.Token);

            if (!string.IsNullOrWhiteSpace(error))
                _logger.LogWarning("Claude CLI stderr: {Error}", error);

            // Build final result
            var result = finalResult ?? new ClaudeResult();

            // Use accumulated text if result text is empty
            if (string.IsNullOrEmpty(result.Text) && accumulatedText.Length > 0)
                result.Text = accumulatedText.ToString();

            result.ToolCalls = toolCalls;

            // Save session ID
            if (!string.IsNullOrEmpty(result.SessionId))
                session.ClaudeSessionId = result.SessionId;

            if (process.ExitCode != 0 && string.IsNullOrEmpty(result.Text))
            {
                result.Text = $"Claude CLI xatosi (exit code {process.ExitCode}):\n{error}";
                result.IsError = true;
            }

            _logger.LogInformation(
                "Claude CLI tugadi. Exit: {ExitCode}, Text: {Len} chars, Tools: {Tools}",
                process.ExitCode, result.Text.Length, toolCalls.Count);

            return result;
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                try { process.Kill(entireProcessTree: true); }
                catch { }
            }

            return new ClaudeResult
            {
                Text = $"So'rov vaqti tugadi ({_claudeSettings.MaxTimeoutSeconds} soniya).",
                IsError = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Claude CLI xatolik");
            return new ClaudeResult
            {
                Text = $"Xatolik: {ex.Message}",
                IsError = true
            };
        }
    }

    private string BuildArguments(string message, UserSession session)
    {
        var sb = new StringBuilder();

        sb.Append("-p ");

        var escapedMessage = message
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");
        sb.Append($"\"{escapedMessage}\" ");

        sb.Append("--output-format stream-json --verbose ");
        sb.Append("--dangerously-skip-permissions ");

        var model = session.Model ?? _claudeSettings.Model;
        sb.Append($"--model {model} ");

        if (!string.IsNullOrEmpty(session.ClaudeSessionId))
            sb.Append($"--resume {session.ClaudeSessionId} ");

        return sb.ToString().Trim();
    }
}
