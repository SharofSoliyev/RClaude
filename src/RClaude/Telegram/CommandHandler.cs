using RClaude.Configuration;
using RClaude.Data;
using RClaude.Permission;
using RClaude.Session;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RClaude.Telegram;

public class CommandHandler
{
    private readonly SessionStore _sessionStore;
    private readonly SessionRepository _repo;
    private readonly PermissionService _permissionService;
    private readonly BotMessages _msg;
    private readonly ILogger<CommandHandler> _logger;

    public CommandHandler(
        SessionStore sessionStore,
        SessionRepository repo,
        PermissionService permissionService,
        BotMessages msg,
        ILogger<CommandHandler> logger)
    {
        _sessionStore = sessionStore;
        _repo = repo;
        _permissionService = permissionService;
        _msg = msg;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(
        ITelegramBotClient bot, Message message, CancellationToken ct)
    {
        var text = message.Text?.Trim();
        if (text == null || !text.StartsWith('/'))
            return false;

        var chatId = message.Chat.Id;
        var userId = message.From?.Id ?? 0;

        var parts = text.Split(' ', 2);
        var command = parts[0].ToLower().Split('@')[0];
        var args = parts.Length > 1 ? parts[1].Trim() : "";

        switch (command)
        {
            case "/start":
                await HandleStart(bot, chatId, ct);
                return true;

            case "/setdir":
                await HandleSetDir(bot, chatId, userId, args, ct);
                return true;

            case "/getdir":
                await HandleGetDir(bot, chatId, userId, ct);
                return true;

            case "/clear":
                await HandleClear(bot, chatId, userId, ct);
                return true;

            case "/model":
                await HandleModel(bot, chatId, userId, args, ct);
                return true;

            case "/session":
                await HandleSessionPicker(bot, chatId, userId, ct);
                return true;

            case "/sessions":
                await HandleSessionsList(bot, chatId, userId, ct);
                return true;

            case "/newsession":
                await HandleNewSession(bot, chatId, userId, args, ct);
                return true;

            case "/renamesession":
                await HandleRenameSession(bot, chatId, userId, args, ct);
                return true;

            case "/deletesession":
                await HandleDeleteSession(bot, chatId, userId, ct);
                return true;

            case "/help":
                await HandleHelp(bot, chatId, ct);
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Handle callback query from inline keyboard (session switch + permission).
    /// </summary>
    public async Task HandleCallbackAsync(
        ITelegramBotClient bot, CallbackQuery callback, CancellationToken ct)
    {
        var data = callback.Data;
        var userId = callback.From.Id;
        var chatId = callback.Message?.Chat.Id ?? 0;

        if (data == null || chatId == 0) return;

        // Permission callbacks: perm:allow:{requestId} or perm:deny:{requestId}
        if (data.StartsWith("perm:"))
        {
            var parts = data.Split(':', 3);
            if (parts.Length == 3)
            {
                var decision = parts[1]; // "allow" or "deny"
                var requestId = parts[2];
                var allowed = decision == "allow";

                var statusText = allowed ? _msg.PermAllowed : _msg.PermDenied;

                // Update message in-place: show status, remove buttons
                try
                {
                    var originalHtml = _permissionService.GetMessageHtml(requestId)
                        ?? Esc(callback.Message?.Text ?? "");

                    await bot.EditMessageText(
                        chatId, callback.Message!.MessageId,
                        $"{originalHtml}\n\n<b>{statusText}</b>",
                        parseMode: ParseMode.Html,
                        replyMarkup: new InlineKeyboardMarkup(Array.Empty<InlineKeyboardButton[]>()),
                        cancellationToken: ct);
                }
                catch { }

                await bot.AnswerCallbackQuery(callback.Id, statusText, cancellationToken: ct);

                // Respond to permission service AFTER UI update
                _permissionService.Respond(requestId, allowed);
            }
            return;
        }

        if (data.StartsWith("switch:"))
        {
            var idStr = data["switch:".Length..];
            if (int.TryParse(idStr, out var sessionId))
            {
                await _repo.SwitchToSession(userId, sessionId);
                var session = await _repo.GetActiveSession(userId);

                var name = session?.Name ?? "?";
                var dir = session?.WorkingDirectory ?? _msg.FolderNotSet;

                await bot.AnswerCallbackQuery(callback.Id,
                    $"{_msg.SessionSwitched}: {name}", cancellationToken: ct);

                await bot.EditMessageText(
                    chatId, callback.Message!.MessageId,
                    $"{_msg.SessionSwitched}: <b>{Esc(name)}</b>\nFolder: <code>{Esc(dir)}</code>",
                    parseMode: ParseMode.Html, cancellationToken: ct);
            }
        }
    }

    // ─── Commands ───────────────────────────────────────

    private async Task HandleStart(
        ITelegramBotClient bot, long chatId, CancellationToken ct)
    {
        await bot.SendMessage(chatId, _msg.StartMessage,
            parseMode: ParseMode.Html, cancellationToken: ct);
    }

    private async Task HandleHelp(
        ITelegramBotClient bot, long chatId, CancellationToken ct)
    {
        await bot.SendMessage(chatId, _msg.HelpMessage,
            parseMode: ParseMode.Html, cancellationToken: ct);
    }

    private async Task HandleSessionPicker(
        ITelegramBotClient bot, long chatId, long userId, CancellationToken ct)
    {
        var sessions = await _repo.GetAllSessions(userId);

        if (sessions.Count == 0)
        {
            await bot.SendMessage(chatId, _msg.NoSessions,
                parseMode: ParseMode.Html, cancellationToken: ct);
            return;
        }

        var buttons = sessions.Select(s =>
        {
            var label = s.IsActive ? $"✅ {s.Name}" : $"📁 {s.Name}";
            return new[] { InlineKeyboardButton.WithCallbackData(label, $"switch:{s.Id}") };
        });

        var keyboard = new InlineKeyboardMarkup(buttons);

        await bot.SendMessage(chatId,
            $"<b>{Esc(_msg.PickSession)}</b>",
            parseMode: ParseMode.Html,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }

    private async Task HandleSessionsList(
        ITelegramBotClient bot, long chatId, long userId, CancellationToken ct)
    {
        var sessions = await _repo.GetAllSessions(userId);

        if (sessions.Count == 0)
        {
            await bot.SendMessage(chatId, _msg.NoSessions,
                parseMode: ParseMode.Html, cancellationToken: ct);
            return;
        }

        var lines = sessions.Select(s =>
        {
            var marker = s.IsActive ? "✅" : "📁";
            var dir = s.WorkingDirectory ?? _msg.FolderNotSet;
            return $"{marker} <b>{Esc(s.Name)}</b>\n   <code>{Esc(dir)}</code>\n   Model: {s.Model} | ID: {s.Id}";
        });

        await bot.SendMessage(chatId,
            $"<b>{Esc(_msg.SessionsTitle)}</b>\n\n" + string.Join("\n\n", lines),
            parseMode: ParseMode.Html, cancellationToken: ct);
    }

    private async Task HandleNewSession(
        ITelegramBotClient bot, long chatId, long userId, string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            await bot.SendMessage(chatId, _msg.NewSessionUsage,
                parseMode: ParseMode.Html, cancellationToken: ct);
            return;
        }

        var session = await _repo.CreateSession(userId, name.Trim());

        _logger.LogInformation("User {UserId} created session: {Name} (ID: {Id})",
            userId, session.Name, session.Id);

        await bot.SendMessage(chatId,
            $"{_msg.NewSessionCreated}: <b>{Esc(session.Name)}</b>\n\n{_msg.SetDirNow}",
            parseMode: ParseMode.Html, cancellationToken: ct);
    }

    private async Task HandleRenameSession(
        ITelegramBotClient bot, long chatId, long userId, string newName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            await bot.SendMessage(chatId, _msg.RenameUsage,
                parseMode: ParseMode.Html, cancellationToken: ct);
            return;
        }

        var active = await _repo.GetActiveSession(userId);
        if (active == null)
        {
            await bot.SendMessage(chatId, _msg.NoActiveSession, cancellationToken: ct);
            return;
        }

        var oldName = active.Name;
        await _repo.UpdateSessionFields(active.Id, e => e.Name = newName.Trim());

        await bot.SendMessage(chatId,
            $"{_msg.SessionRenamed}:\n<b>{Esc(oldName)}</b> → <b>{Esc(newName.Trim())}</b>",
            parseMode: ParseMode.Html, cancellationToken: ct);
    }

    private async Task HandleDeleteSession(
        ITelegramBotClient bot, long chatId, long userId, CancellationToken ct)
    {
        var active = await _repo.GetActiveSession(userId);
        if (active == null)
        {
            await bot.SendMessage(chatId, _msg.NoSessionToDelete, cancellationToken: ct);
            return;
        }

        var name = active.Name;
        await _repo.DeleteSession(active.Id);

        var newActive = await _repo.GetActiveSession(userId);
        var text = $"{_msg.SessionDeleted}: <b>{Esc(name)}</b>";
        if (newActive != null)
            text += $"\n\n{_msg.CurrentSession}: <b>{Esc(newActive.Name)}</b>";
        else
            text += $"\n\n{_msg.NoSessionsLeft}";

        await bot.SendMessage(chatId, text,
            parseMode: ParseMode.Html, cancellationToken: ct);
    }

    private async Task HandleSetDir(
        ITelegramBotClient bot, long chatId, long userId, string path, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            await bot.SendMessage(chatId, _msg.SetDirUsage, cancellationToken: ct);
            return;
        }

        var fullPath = Path.GetFullPath(path);

        if (!Directory.Exists(fullPath))
        {
            await bot.SendMessage(chatId,
                $"{_msg.FolderNotFound}: <code>{Esc(fullPath)}</code>",
                parseMode: ParseMode.Html, cancellationToken: ct);
            return;
        }

        var active = await _repo.GetActiveSession(userId);
        if (active == null)
        {
            active = await _repo.EnsureDefaultSession(userId);
        }

        await _repo.UpdateSessionFields(active.Id, e =>
        {
            e.WorkingDirectory = fullPath;
            e.ClaudeSessionId = null; // Yangi folder = yangi context
        });

        _logger.LogInformation("User {UserId} set dir: {Dir} (session: {Session})",
            userId, fullPath, active.Name);

        await bot.SendMessage(chatId,
            $"Sessiya: <b>{Esc(active.Name)}</b>\nFolder: <code>{Esc(fullPath)}</code>",
            parseMode: ParseMode.Html, cancellationToken: ct);
    }

    private async Task HandleGetDir(
        ITelegramBotClient bot, long chatId, long userId, CancellationToken ct)
    {
        var active = await _repo.GetActiveSession(userId);

        if (active == null)
        {
            await bot.SendMessage(chatId, _msg.NoActiveSession,
                cancellationToken: ct);
            return;
        }

        var dir = active.WorkingDirectory ?? _msg.FolderNotSet;
        await bot.SendMessage(chatId,
            $"Sessiya: <b>{Esc(active.Name)}</b>\nFolder: <code>{Esc(dir)}</code>\nModel: <b>{active.Model}</b>",
            parseMode: ParseMode.Html, cancellationToken: ct);
    }

    private async Task HandleClear(
        ITelegramBotClient bot, long chatId, long userId, CancellationToken ct)
    {
        await _sessionStore.ClearSessionAsync(userId);

        await bot.SendMessage(chatId, _msg.ContextCleared,
            cancellationToken: ct);
    }

    private async Task HandleModel(
        ITelegramBotClient bot, long chatId, long userId, string modelName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            await bot.SendMessage(chatId, _msg.ModelUsage,
                parseMode: ParseMode.Html, cancellationToken: ct);
            return;
        }

        var model = modelName.ToLower().Trim();
        var validModels = new[] { "sonnet", "opus", "haiku" };

        if (!validModels.Contains(model))
        {
            await bot.SendMessage(chatId,
                $"{_msg.InvalidModel}: <code>{Esc(model)}</code>",
                parseMode: ParseMode.Html, cancellationToken: ct);
            return;
        }

        var active = await _repo.GetActiveSession(userId);
        if (active != null)
            await _repo.UpdateSessionFields(active.Id, e => e.Model = model);

        await bot.SendMessage(chatId,
            $"{_msg.ModelChanged}: <b>{model}</b>",
            parseMode: ParseMode.Html, cancellationToken: ct);
    }

    private static string Esc(string text) =>
        text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
