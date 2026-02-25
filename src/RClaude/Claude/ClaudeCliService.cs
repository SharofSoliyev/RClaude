using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Options;
using RClaude.Configuration;
using RClaude.Permission;
using RClaude.Session;

namespace RClaude.Claude;

public class ClaudeCliService
{
    private readonly ClaudeSettings _claudeSettings;
    private readonly PermissionService _permissionService;
    private readonly BotMessages _msg;
    private readonly ILogger<ClaudeCliService> _logger;

    public ClaudeCliService(
        IOptions<ClaudeSettings> claudeSettings,
        PermissionService permissionService,
        BotMessages msg,
        ILogger<ClaudeCliService> logger)
    {
        _claudeSettings = claudeSettings.Value;
        _permissionService = permissionService;
        _msg = msg;
        _logger = logger;
    }

    /// <summary>
    /// Send a message to Claude CLI with real-time streaming events.
    /// onEvent is called for each stream event (text delta, tool use, result).
    /// </summary>
    public async Task<ClaudeResult> SendMessageAsync(
        string message,
        UserSession session,
        long chatId,
        Func<StreamEvent, Task>? onEvent = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(session.WorkingDirectory))
        {
            return new ClaudeResult
            {
                Text = _msg.NoWorkingDir,
                IsError = true
            };
        }

        var psi = new ProcessStartInfo
        {
            FileName = _claudeSettings.CliBinaryPath,
            WorkingDirectory = session.WorkingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        // ArgumentList — har bir argument alohida uzatiladi, shell injection imkonsiz
        BuildArgumentList(psi.ArgumentList, message, session);
        _logger.LogInformation("Claude CLI: {Binary} with {ArgCount} args", _claudeSettings.CliBinaryPath, psi.ArgumentList.Count);

        // Remove CLAUDECODE env var to prevent "nested session" error
        psi.Environment.Remove("CLAUDECODE");
        psi.Environment.Remove("CLAUDE_CODE_ENTRYPOINT");

        // Permission service env vars — hook script reads these
        psi.Environment["RCLAUDE_PERMISSION_PORT"] = _permissionService.Port.ToString();
        psi.Environment["RCLAUDE_USER_ID"] = session.TelegramUserId.ToString();
        psi.Environment["RCLAUDE_CHAT_ID"] = chatId.ToString();
        psi.Environment["RCLAUDE_PERMISSION_MODE"] = _claudeSettings.PermissionMode;

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
                result.Text = $"{_msg.CliError} (exit code {process.ExitCode}):\n{error}";
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
                Text = $"{_msg.Timeout} ({_claudeSettings.MaxTimeoutSeconds}s)",
                IsError = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Claude CLI xatolik");
            return new ClaudeResult
            {
                Text = $"{_msg.GenericError}: {ex.Message}",
                IsError = true
            };
        }
    }

    private static readonly HashSet<string> ValidModels = new(StringComparer.OrdinalIgnoreCase)
    {
        "sonnet", "opus", "haiku",
        "claude-sonnet-4-5-20250514", "claude-opus-4-5-20250514",
        "claude-3-5-sonnet-20241022", "claude-3-5-haiku-20241022"
    };

    private static readonly System.Text.RegularExpressions.Regex SessionIdPattern =
        new(@"^[a-zA-Z0-9\-_]+$", System.Text.RegularExpressions.RegexOptions.Compiled);

    private void BuildArgumentList(IList<string> args, string message, UserSession session)
    {
        // -p <message> — xabar to'g'ridan-to'g'ri argument sifatida, shell parsing yo'q
        args.Add("-p");
        args.Add(message);

        args.Add("--output-format");
        args.Add("stream-json");

        args.Add("--verbose");

        // Model validatsiyasi — faqat ruxsat etilgan nomlar
        var model = session.Model ?? _claudeSettings.Model;
        if (!ValidModels.Contains(model))
            model = "sonnet";
        args.Add("--model");
        args.Add(model);

        // Session ID validatsiyasi — faqat alphanumeric, dash, underscore
        if (!string.IsNullOrEmpty(session.ClaudeSessionId)
            && SessionIdPattern.IsMatch(session.ClaudeSessionId))
        {
            args.Add("--resume");
            args.Add(session.ClaudeSessionId);
        }
    }
}
