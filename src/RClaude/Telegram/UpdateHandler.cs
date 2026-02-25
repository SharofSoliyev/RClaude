using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RClaude.AudioProcessing;
using RClaude.Claude;
using RClaude.Configuration;
using RClaude.Permission;
using RClaude.Session;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RClaude.Telegram;

public class UpdateHandler
{
    private readonly CommandHandler _commandHandler;
    private readonly ClaudeCliService _claudeService;
    private readonly SessionStore _sessionStore;
    private readonly MessageFormatter _formatter;
    private readonly PermissionService _permissionService;
    private readonly WhisperService _whisperService;
    private readonly PromptOptimizer _promptOptimizer;
    private readonly BotMessages _msg;
    private readonly TelegramSettings _telegramSettings;
    private readonly ILogger<UpdateHandler> _logger;

    // Store pending audio prompts for confirmation
    private readonly Dictionary<string, string> _pendingAudioPrompts = new();

    private static readonly TimeSpan EditInterval = TimeSpan.FromMilliseconds(1200);
    private const int MaxTelegramLength = 4000;

    public UpdateHandler(
        CommandHandler commandHandler,
        ClaudeCliService claudeService,
        SessionStore sessionStore,
        MessageFormatter formatter,
        PermissionService permissionService,
        WhisperService whisperService,
        PromptOptimizer promptOptimizer,
        BotMessages msg,
        IOptions<TelegramSettings> telegramSettings,
        ILogger<UpdateHandler> logger)
    {
        _commandHandler = commandHandler;
        _claudeService = claudeService;
        _sessionStore = sessionStore;
        _formatter = formatter;
        _permissionService = permissionService;
        _whisperService = whisperService;
        _promptOptimizer = promptOptimizer;
        _msg = msg;
        _telegramSettings = telegramSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Register the permission event handler with a bot reference.
    /// Called once when TelegramHostedService starts.
    /// </summary>
    public void RegisterPermissionHandler(ITelegramBotClient bot)
    {
        _permissionService.OnPermissionRequested += async (request) =>
        {
            await SendPermissionButtons(bot, request);
        };
    }

    public async Task HandleUpdateAsync(
        ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        // Handle callback queries (inline keyboard buttons)
        if (update.CallbackQuery is { } callback)
        {
            if (IsAuthorized(callback.From.Id, callback.From.Username))
            {
                // Check if it's an audio callback
                if (callback.Data?.StartsWith("audio:") == true)
                {
                    await HandleAudioCallbackAsync(bot, callback, ct);
                    return;
                }

                await _commandHandler.HandleCallbackAsync(bot, callback, ct);
            }
            return;
        }

        // Handle voice/audio messages
        if (update.Message is { Voice: { } voice } voiceMessage)
        {
            if (IsAuthorized(voiceMessage.From?.Id ?? 0, voiceMessage.From?.Username))
                await HandleVoiceMessageAsync(bot, voiceMessage, voice, ct);
            return;
        }

        if (update.Message is { Audio: { } audio } audioMessage)
        {
            if (IsAuthorized(audioMessage.From?.Id ?? 0, audioMessage.From?.Username))
                await HandleAudioMessageAsync(bot, audioMessage, audio, ct);
            return;
        }

        if (update.Message is not { Text: { } text } message)
            return;

        var chatId = message.Chat.Id;
        var userId = message.From?.Id ?? 0;

        if (!IsAuthorized(userId, message.From?.Username))
        {
            _logger.LogWarning("Unauthorized: {UserId} (@{Username})",
                userId, message.From?.Username);
            await bot.SendMessage(chatId, _msg.Unauthorized,
                cancellationToken: ct);
            return;
        }

        if (text.StartsWith('/'))
        {
            var handled = await _commandHandler.HandleAsync(bot, message, ct);
            if (handled) return;

            await bot.SendMessage(chatId, _msg.UnknownCommand,
                cancellationToken: ct);
            return;
        }

        // Load session from DB
        var session = await _sessionStore.GetOrCreateAsync(userId);

        if (session.WorkingDirectory == null)
        {
            await bot.SendMessage(chatId, _msg.SetDirFirst,
                parseMode: ParseMode.Html, cancellationToken: ct);
            return;
        }

        // Per-user lock
        var userLock = _sessionStore.GetLock(userId);
        if (!await userLock.WaitAsync(0, ct))
        {
            await bot.SendMessage(chatId, _msg.RequestInProgress,
                cancellationToken: ct);
            return;
        }

        try
        {
            await ProcessWithStreaming(bot, chatId, text, session, ct);

            // Save Claude session ID back to DB
            await _sessionStore.SaveSessionAsync(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error from {UserId}", userId);
            await bot.SendMessage(chatId,
                $"{_msg.ErrorOccurred}: {ex.Message}", cancellationToken: ct);
        }
        finally
        {
            userLock.Release();
        }
    }

    private async Task ProcessWithStreaming(
        ITelegramBotClient bot, long chatId, string text,
        UserSession session, CancellationToken ct)
    {
        var liveMsg = await bot.SendMessage(chatId,
            $"⏳ <i>{_msg.Thinking} [{session.SessionName}]</i>",
            parseMode: ParseMode.Html, cancellationToken: ct);

        var liveMessageId = liveMsg.MessageId;
        var accumulatedText = new StringBuilder();
        var currentTools = new List<string>();
        var lastEditTime = DateTime.MinValue;

        var result = await _claudeService.SendMessageAsync(
            text, session, chatId,
            onEvent: async (streamEvent) =>
            {
                switch (streamEvent)
                {
                    case StreamEvent.TextDelta delta:
                        accumulatedText.Append(delta.Text);

                        if (DateTime.UtcNow - lastEditTime >= EditInterval)
                        {
                            var displayText = TruncateForTelegram(accumulatedText.ToString());
                            var html = MessageFormatter.ConvertMarkdownToHtml(displayText) + " ▊";
                            await TryEditMessage(bot, chatId, liveMessageId, html, ct);
                            lastEditTime = DateTime.UtcNow;
                        }
                        break;

                    case StreamEvent.ToolUse tool:
                        currentTools.Add(tool.ToolName);

                        if (DateTime.UtcNow - lastEditTime >= EditInterval)
                        {
                            var toolStatus = GetToolEmoji(tool.ToolName)
                                + " <i>" + tool.ToolName + " " + _msg.PermToolRunning + "</i>";

                            var display = accumulatedText.Length > 0
                                ? MessageFormatter.ConvertMarkdownToHtml(
                                    TruncateForTelegram(accumulatedText.ToString()))
                                    + "\n\n" + toolStatus
                                : toolStatus;

                            await TryEditMessage(bot, chatId, liveMessageId, display, ct);
                            lastEditTime = DateTime.UtcNow;
                        }
                        break;
                }
            },
            ct);

        var finalText = !string.IsNullOrEmpty(result.Text)
            ? result.Text
            : accumulatedText.ToString();

        if (string.IsNullOrWhiteSpace(finalText))
            finalText = _msg.EmptyResponse;

        var footer = BuildFooter(result);

        if (finalText.Length + footer.Length > MaxTelegramLength)
        {
            try { await bot.DeleteMessage(chatId, liveMessageId, cancellationToken: ct); }
            catch { }

            await _formatter.SendResponseAsync(bot, chatId, finalText + footer, ct);
        }
        else
        {
            var finalHtml = MessageFormatter.ConvertMarkdownToHtml(finalText) + footer;
            await TryEditMessage(bot, chatId, liveMessageId, finalHtml, ct);
        }
    }

    private async Task SendPermissionButtons(ITelegramBotClient bot, PermissionRequest request)
    {
        var emoji = GetToolEmoji(request.ToolName);
        var detail = ExtractToolDetail(request);

        var text = $"{emoji} <b>{Esc(request.ToolName)}</b> {_msg.PermWantsToUse}:\n"
                 + $"<code>{Esc(detail)}</code>";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData(_msg.PermBtnAllow, $"perm:allow:{request.RequestId}"),
            InlineKeyboardButton.WithCallbackData(_msg.PermBtnDeny, $"perm:deny:{request.RequestId}")
        });

        // Store original HTML so callback handler can reuse it
        _permissionService.StoreMessageHtml(request.RequestId, text);

        try
        {
            await bot.SendMessage(request.ChatId, text,
                parseMode: ParseMode.Html,
                replyMarkup: keyboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send permission buttons for {RequestId}", request.RequestId);
        }
    }

    private static string ExtractToolDetail(PermissionRequest request)
    {
        if (request.ToolInput == null)
            return request.ToolName;

        try
        {
            JsonElement input;
            if (request.ToolInput is JsonElement je)
                input = je;
            else
                input = JsonSerializer.Deserialize<JsonElement>(
                    JsonSerializer.Serialize(request.ToolInput));

            return request.ToolName switch
            {
                "Bash" or "bash" =>
                    input.TryGetProperty("command", out var cmd) ? cmd.GetString() ?? "?"
                    : input.TryGetProperty("tool_input", out var ti)
                        && ti.TryGetProperty("command", out var cmd2) ? cmd2.GetString() ?? "?" : "?",

                "Write" or "write_file" =>
                    input.TryGetProperty("file_path", out var fp) ? $"📄 {fp.GetString()}"
                    : input.TryGetProperty("tool_input", out var ti2)
                        && ti2.TryGetProperty("file_path", out var fp2) ? $"📄 {fp2.GetString()}" : "?",

                "Edit" or "edit_file" =>
                    input.TryGetProperty("file_path", out var ep) ? $"📄 {ep.GetString()}"
                    : input.TryGetProperty("tool_input", out var ti3)
                        && ti3.TryGetProperty("file_path", out var ep2) ? $"📄 {ep2.GetString()}" : "?",

                _ => request.ToolName
            };
        }
        catch
        {
            return request.ToolName;
        }
    }

    public static string GetToolEmoji(string toolName) => toolName switch
    {
        "Read" or "read_file" => "📖",
        "Write" or "write_file" => "✍️",
        "Edit" or "edit_file" => "✏️",
        "Bash" or "bash" => "💻",
        "Glob" or "glob" => "🔍",
        "Grep" or "grep" => "🔎",
        "ListDirectory" or "list_directory" => "📁",
        "TodoWrite" => "📋",
        "WebSearch" or "WebFetch" => "🌐",
        _ => "🔧"
    };

    private static string BuildFooter(ClaudeResult result)
    {
        var parts = new List<string>();

        if (result.CostUsd.HasValue)
            parts.Add($"${result.CostUsd:F4}");

        if (result.DurationMs.HasValue)
            parts.Add($"{result.DurationMs / 1000.0:F1}s");

        if (result.ToolCalls.Count > 0)
            parts.Add(string.Join(", ", result.ToolCalls.Distinct()));

        return parts.Count > 0
            ? $"\n\n<i>{string.Join(" | ", parts)}</i>"
            : "";
    }

    private static string TruncateForTelegram(string text)
    {
        return text.Length > MaxTelegramLength - 200
            ? text[..(MaxTelegramLength - 200)] + "\n..."
            : text;
    }

    private async Task TryEditMessage(
        ITelegramBotClient bot, long chatId, int messageId,
        string htmlText, CancellationToken ct)
    {
        try
        {
            await bot.EditMessageText(chatId, messageId, htmlText,
                parseMode: ParseMode.Html, cancellationToken: ct);
        }
        catch (Exception ex) when (
            ex.Message.Contains("message is not modified", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("MESSAGE_NOT_MODIFIED", StringComparison.OrdinalIgnoreCase))
        {
        }
        catch (Exception ex)
        {
            _logger.LogDebug("HTML edit failed: {Error}", ex.Message);
            try
            {
                var plain = MessageFormatter.StripHtmlTags(htmlText);
                await bot.EditMessageText(chatId, messageId, plain,
                    cancellationToken: ct);
            }
            catch { }
        }
    }

    private bool IsAuthorized(long userId, string? username)
    {
        var hasIdFilter = _telegramSettings.AllowedUserIds.Length > 0;
        var hasUsernameFilter = _telegramSettings.AllowedUsernames.Length > 0;

        if (!hasIdFilter && !hasUsernameFilter)
            return true;

        if (hasIdFilter && _telegramSettings.AllowedUserIds.Contains(userId))
            return true;

        if (hasUsernameFilter && !string.IsNullOrEmpty(username))
        {
            if (_telegramSettings.AllowedUsernames
                .Any(u => u.TrimStart('@').Equals(username.TrimStart('@'), StringComparison.OrdinalIgnoreCase)))
                return true;
        }

        return false;
    }

    private static string Esc(string text) =>
        text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

    // ─── Audio Message Handlers ─────────────────────

    private async Task HandleVoiceMessageAsync(
        ITelegramBotClient bot, Message message, Voice voice, CancellationToken ct)
    {
        var chatId = message.Chat.Id;

        if (!_whisperService.IsAvailable)
        {
            await bot.SendMessage(chatId, _msg.AudioNotAvailable, cancellationToken: ct);
            return;
        }

        // Check duration
        if (!_whisperService.IsValidDuration(voice.Duration))
        {
            await bot.SendMessage(chatId,
                $"{_msg.AudioTooLong}: {_whisperService.MaxDurationSeconds} soniya.",
                cancellationToken: ct);
            return;
        }

        await ProcessAudioAsync(bot, chatId, message.From?.Id ?? 0, voice.FileId, ct);
    }

    private async Task HandleAudioMessageAsync(
        ITelegramBotClient bot, Message message, Audio audio, CancellationToken ct)
    {
        var chatId = message.Chat.Id;

        if (!_whisperService.IsAvailable)
        {
            await bot.SendMessage(chatId, _msg.AudioNotAvailable, cancellationToken: ct);
            return;
        }

        // Check duration
        if (!_whisperService.IsValidDuration(audio.Duration))
        {
            await bot.SendMessage(chatId,
                $"{_msg.AudioTooLong}: {_whisperService.MaxDurationSeconds} soniya.",
                cancellationToken: ct);
            return;
        }

        await ProcessAudioAsync(bot, chatId, message.From?.Id ?? 0, audio.FileId, ct);
    }

    private async Task ProcessAudioAsync(
        ITelegramBotClient bot, long chatId, long userId, string fileId, CancellationToken ct)
    {
        var statusMsg = await bot.SendMessage(chatId,
            $"⏳ <i>{_msg.AudioProcessing}</i>",
            parseMode: ParseMode.Html, cancellationToken: ct);

        string? tempFilePath = null;
        try
        {
            // Download audio file
            var file = await bot.GetFile(fileId, cancellationToken: ct);
            if (file.FilePath == null)
            {
                await bot.EditMessageText(chatId, statusMsg.MessageId,
                    _msg.AudioTranscriptionFailed, cancellationToken: ct);
                return;
            }

            // Save to temp file with proper extension
            // Telegram sends voice as OGG Opus, but use .ogg extension for Whisper
            var extension = file.FilePath.EndsWith(".oga") ? ".oga" : ".ogg";
            tempFilePath = Path.Combine(Path.GetTempPath(), $"rclaude_audio_{Guid.NewGuid()}{extension}");

            _logger.LogInformation("Downloading audio to: {TempPath}", tempFilePath);

            await using (var fileStream = System.IO.File.Create(tempFilePath))
            {
                await bot.DownloadFile(file.FilePath, fileStream, cancellationToken: ct);
            }

            var fileSize = new FileInfo(tempFilePath).Length;
            _logger.LogInformation("Downloaded audio file: {Size} bytes", fileSize);

            // Transcribe with Whisper
            var transcription = await _whisperService.TranscribeAsync(tempFilePath, ct);
            if (string.IsNullOrWhiteSpace(transcription))
            {
                await bot.EditMessageText(chatId, statusMsg.MessageId,
                    _msg.AudioTranscriptionFailed, cancellationToken: ct);
                return;
            }

            // Optimize prompt with GPT
            string optimizedPrompt;
            if (_promptOptimizer.IsAvailable)
            {
                optimizedPrompt = await _promptOptimizer.OptimizeAsync(transcription, ct)
                                  ?? transcription;
            }
            else
            {
                optimizedPrompt = transcription;
            }

            // Show confirmation dialog
            var callbackId = Guid.NewGuid().ToString("N");
            _pendingAudioPrompts[callbackId] = optimizedPrompt;

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData(_msg.AudioBtnSend, $"audio:send:{callbackId}"),
                InlineKeyboardButton.WithCallbackData(_msg.AudioBtnCancel, $"audio:cancel:{callbackId}")
            });

            var displayText = $"<b>{_msg.AudioTranscribed}:</b>\n<code>{Esc(transcription)}</code>";

            if (optimizedPrompt != transcription && _promptOptimizer.IsAvailable)
            {
                displayText += $"\n\n<b>{_msg.AudioOptimized}:</b>\n<code>{Esc(optimizedPrompt)}</code>";
            }

            displayText += $"\n\n{_msg.AudioSendConfirm}";

            await bot.EditMessageText(chatId, statusMsg.MessageId, displayText,
                parseMode: ParseMode.Html, replyMarkup: keyboard, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing audio for user {UserId}", userId);
            await bot.EditMessageText(chatId, statusMsg.MessageId,
                $"{_msg.ErrorOccurred}: {ex.Message}", cancellationToken: ct);
        }
        finally
        {
            // Clean up temp file
            if (tempFilePath != null && System.IO.File.Exists(tempFilePath))
            {
                try { System.IO.File.Delete(tempFilePath); }
                catch { }
            }
        }
    }

    private async Task HandleAudioCallbackAsync(
        ITelegramBotClient bot, CallbackQuery callback, CancellationToken ct)
    {
        var data = callback.Data;
        var userId = callback.From.Id;
        var chatId = callback.Message?.Chat.Id ?? 0;

        if (data == null || chatId == 0) return;

        var parts = data.Split(':', 3);
        if (parts.Length != 3) return;

        var action = parts[1]; // "send" or "cancel"
        var callbackId = parts[2];

        if (!_pendingAudioPrompts.TryGetValue(callbackId, out var prompt))
        {
            await bot.AnswerCallbackQuery(callback.Id, "Prompt expired", cancellationToken: ct);
            return;
        }

        // Remove from pending
        _pendingAudioPrompts.Remove(callbackId);

        if (action == "cancel")
        {
            await bot.EditMessageText(chatId, callback.Message!.MessageId,
                $"<i>{_msg.AudioCancelled}</i>",
                parseMode: ParseMode.Html, cancellationToken: ct);
            await bot.AnswerCallbackQuery(callback.Id, _msg.AudioCancelled, cancellationToken: ct);
            return;
        }

        if (action == "send")
        {
            // Remove confirmation message
            try
            {
                await bot.DeleteMessage(chatId, callback.Message!.MessageId, cancellationToken: ct);
            }
            catch { }

            await bot.AnswerCallbackQuery(callback.Id, "✅", cancellationToken: ct);

            // Load session and process
            var session = await _sessionStore.GetOrCreateAsync(userId);

            if (session.WorkingDirectory == null)
            {
                await bot.SendMessage(chatId, _msg.SetDirFirst,
                    parseMode: ParseMode.Html, cancellationToken: ct);
                return;
            }

            // Per-user lock
            var userLock = _sessionStore.GetLock(userId);
            if (!await userLock.WaitAsync(0, ct))
            {
                await bot.SendMessage(chatId, _msg.RequestInProgress, cancellationToken: ct);
                return;
            }

            try
            {
                await ProcessWithStreaming(bot, chatId, prompt, session, ct);
                await _sessionStore.SaveSessionAsync(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing audio prompt from {UserId}", userId);
                await bot.SendMessage(chatId, $"{_msg.ErrorOccurred}: {ex.Message}", cancellationToken: ct);
            }
            finally
            {
                userLock.Release();
            }
        }
    }
}
