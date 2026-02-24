using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
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
    private readonly TelegramSettings _telegramSettings;
    private readonly ILogger<UpdateHandler> _logger;

    private static readonly TimeSpan EditInterval = TimeSpan.FromMilliseconds(1200);
    private const int MaxTelegramLength = 4000;

    public UpdateHandler(
        CommandHandler commandHandler,
        ClaudeCliService claudeService,
        SessionStore sessionStore,
        MessageFormatter formatter,
        PermissionService permissionService,
        IOptions<TelegramSettings> telegramSettings,
        ILogger<UpdateHandler> logger)
    {
        _commandHandler = commandHandler;
        _claudeService = claudeService;
        _sessionStore = sessionStore;
        _formatter = formatter;
        _permissionService = permissionService;
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
                await _commandHandler.HandleCallbackAsync(bot, callback, ct);
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
            await bot.SendMessage(chatId,
                "Ruxsat berilmagan. Administrator bilan bog'laning.",
                cancellationToken: ct);
            return;
        }

        if (text.StartsWith('/'))
        {
            var handled = await _commandHandler.HandleAsync(bot, message, ct);
            if (handled) return;

            await bot.SendMessage(chatId,
                "Noma'lum buyruq. /help ni ko'ring.",
                cancellationToken: ct);
            return;
        }

        // Load session from DB
        var session = await _sessionStore.GetOrCreateAsync(userId);

        if (session.WorkingDirectory == null)
        {
            await bot.SendMessage(chatId,
                "Avval folder belgilang:\n/setdir /path/to/project\n\nYoki yangi sessiya: /newsession &lt;nom&gt;",
                parseMode: ParseMode.Html, cancellationToken: ct);
            return;
        }

        // Per-user lock
        var userLock = _sessionStore.GetLock(userId);
        if (!await userLock.WaitAsync(0, ct))
        {
            await bot.SendMessage(chatId,
                "Oldingi so'rov hali bajarilmoqda. Iltimos kuting...",
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
                $"Xatolik yuz berdi: {ex.Message}", cancellationToken: ct);
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
            $"⏳ <i>O'ylamoqda... [{session.SessionName}]</i>",
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
                                + " <i>" + tool.ToolName + " ishlatilmoqda...</i>";

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
            finalText = "(bo'sh javob)";

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

        var text = $"{emoji} <b>{Esc(request.ToolName)}</b> ishlatmoqchi:\n"
                 + $"<code>{Esc(detail)}</code>";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("✅ Ruxsat berish", $"perm:allow:{request.RequestId}"),
            InlineKeyboardButton.WithCallbackData("❌ Rad etish", $"perm:deny:{request.RequestId}")
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
}
