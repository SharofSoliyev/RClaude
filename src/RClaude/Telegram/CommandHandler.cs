using RClaude.Data;
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
    private readonly ILogger<CommandHandler> _logger;

    public CommandHandler(
        SessionStore sessionStore,
        SessionRepository repo,
        ILogger<CommandHandler> logger)
    {
        _sessionStore = sessionStore;
        _repo = repo;
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
    /// Handle callback query from inline keyboard (session switch).
    /// </summary>
    public async Task HandleCallbackAsync(
        ITelegramBotClient bot, CallbackQuery callback, CancellationToken ct)
    {
        var data = callback.Data;
        var userId = callback.From.Id;
        var chatId = callback.Message?.Chat.Id ?? 0;

        if (data == null || chatId == 0) return;

        if (data.StartsWith("switch:"))
        {
            var idStr = data["switch:".Length..];
            if (int.TryParse(idStr, out var sessionId))
            {
                await _repo.SwitchToSession(userId, sessionId);
                var session = await _repo.GetActiveSession(userId);

                var name = session?.Name ?? "?";
                var dir = session?.WorkingDirectory ?? "belgilanmagan";

                await bot.AnswerCallbackQuery(callback.Id,
                    $"Sessiya: {name}", cancellationToken: ct);

                await bot.EditMessageText(
                    chatId, callback.Message!.MessageId,
                    $"Sessiya almashtirildi: <b>{Esc(name)}</b>\nFolder: <code>{Esc(dir)}</code>",
                    parseMode: ParseMode.Html, cancellationToken: ct);
            }
        }
    }

    // ─── Commands ───────────────────────────────────────

    private static async Task HandleStart(
        ITelegramBotClient bot, long chatId, CancellationToken ct)
    {
        await bot.SendMessage(chatId, """
            <b>RClaude — Claude Code Agent</b>

            Telegram orqali kodlash yordamchisi!
            Xuddi VS Code extensiondek ishlaydi.

            <b>Sessiya buyruqlari:</b>
            /session — Sessiya tanlash (inline keyboard)
            /sessions — Barcha sessiyalar ro'yxati
            /newsession &lt;nom&gt; — Yangi sessiya yaratish
            /renamesession &lt;nom&gt; — Sessiya nomini o'zgartirish
            /deletesession — Hozirgi sessiyani o'chirish

            <b>Ish buyruqlari:</b>
            /setdir &lt;path&gt; — Folder belgilash
            /getdir — Hozirgi ma'lumotlar
            /clear — Suhbatni tozalash
            /model &lt;name&gt; — Model (sonnet/opus/haiku)
            /help — Yordam

            Boshlash: /newsession MyProject → /setdir /path → xabar yozing!
            """, parseMode: ParseMode.Html, cancellationToken: ct);
    }

    private static async Task HandleHelp(
        ITelegramBotClient bot, long chatId, CancellationToken ct)
    {
        await bot.SendMessage(chatId, """
            <b>RClaude Buyruqlar:</b>

            <b>Sessiyalar:</b>
            /session — Sessiya tanlash (button)
            /sessions — Barcha sessiyalar ro'yxati
            /newsession &lt;nom&gt; — Yangi sessiya
            /renamesession &lt;nom&gt; — Nom o'zgartirish
            /deletesession — Sessiya o'chirish

            <b>Ishlash:</b>
            /setdir &lt;path&gt; — Folder belgilash
            /getdir — Hozirgi sessiya ma'lumotlari
            /clear — Context tozalash
            /model &lt;name&gt; — sonnet, opus, haiku

            <b>Qanday ishlaydi:</b>
            Xabar yuboring — Claude Code fayllarni o'qiydi, yozadi, tahrirlaydi, shell buyruqlar bajaradi.
            """, parseMode: ParseMode.Html, cancellationToken: ct);
    }

    private async Task HandleSessionPicker(
        ITelegramBotClient bot, long chatId, long userId, CancellationToken ct)
    {
        var sessions = await _repo.GetAllSessions(userId);

        if (sessions.Count == 0)
        {
            await bot.SendMessage(chatId,
                "Sessiyalar yo'q. /newsession &lt;nom&gt; bilan yarating.",
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
            "<b>Sessiya tanlang:</b>",
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
            await bot.SendMessage(chatId,
                "Sessiyalar yo'q. /newsession &lt;nom&gt; bilan yarating.",
                parseMode: ParseMode.Html, cancellationToken: ct);
            return;
        }

        var lines = sessions.Select(s =>
        {
            var marker = s.IsActive ? "✅" : "📁";
            var dir = s.WorkingDirectory ?? "folder belgilanmagan";
            return $"{marker} <b>{Esc(s.Name)}</b>\n   <code>{Esc(dir)}</code>\n   Model: {s.Model} | ID: {s.Id}";
        });

        await bot.SendMessage(chatId,
            "<b>Sessiyalar:</b>\n\n" + string.Join("\n\n", lines),
            parseMode: ParseMode.Html, cancellationToken: ct);
    }

    private async Task HandleNewSession(
        ITelegramBotClient bot, long chatId, long userId, string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            await bot.SendMessage(chatId,
                "Foydalanish: /newsession &lt;nom&gt;\nMasalan: /newsession Wordy Backend",
                parseMode: ParseMode.Html, cancellationToken: ct);
            return;
        }

        var session = await _repo.CreateSession(userId, name.Trim());

        _logger.LogInformation("User {UserId} created session: {Name} (ID: {Id})",
            userId, session.Name, session.Id);

        await bot.SendMessage(chatId,
            $"Yangi sessiya yaratildi: <b>{Esc(session.Name)}</b>\n\nEndi /setdir bilan folder belgilang.",
            parseMode: ParseMode.Html, cancellationToken: ct);
    }

    private async Task HandleRenameSession(
        ITelegramBotClient bot, long chatId, long userId, string newName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            await bot.SendMessage(chatId,
                "Foydalanish: /renamesession &lt;yangi nom&gt;",
                parseMode: ParseMode.Html, cancellationToken: ct);
            return;
        }

        var active = await _repo.GetActiveSession(userId);
        if (active == null)
        {
            await bot.SendMessage(chatId, "Active sessiya yo'q.", cancellationToken: ct);
            return;
        }

        var oldName = active.Name;
        await _repo.UpdateSessionFields(active.Id, e => e.Name = newName.Trim());

        await bot.SendMessage(chatId,
            $"Sessiya nomi o'zgartirildi:\n<b>{Esc(oldName)}</b> → <b>{Esc(newName.Trim())}</b>",
            parseMode: ParseMode.Html, cancellationToken: ct);
    }

    private async Task HandleDeleteSession(
        ITelegramBotClient bot, long chatId, long userId, CancellationToken ct)
    {
        var active = await _repo.GetActiveSession(userId);
        if (active == null)
        {
            await bot.SendMessage(chatId, "O'chirish uchun sessiya yo'q.", cancellationToken: ct);
            return;
        }

        var name = active.Name;
        await _repo.DeleteSession(active.Id);

        var newActive = await _repo.GetActiveSession(userId);
        var msg = $"Sessiya o'chirildi: <b>{Esc(name)}</b>";
        if (newActive != null)
            msg += $"\n\nHozirgi sessiya: <b>{Esc(newActive.Name)}</b>";
        else
            msg += "\n\nSessiyalar qolmadi. /newsession bilan yarating.";

        await bot.SendMessage(chatId, msg,
            parseMode: ParseMode.Html, cancellationToken: ct);
    }

    private async Task HandleSetDir(
        ITelegramBotClient bot, long chatId, long userId, string path, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            await bot.SendMessage(chatId,
                "Foydalanish: /setdir /path/to/project", cancellationToken: ct);
            return;
        }

        var fullPath = Path.GetFullPath(path);

        if (!Directory.Exists(fullPath))
        {
            await bot.SendMessage(chatId,
                $"Papka topilmadi: <code>{Esc(fullPath)}</code>",
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
            await bot.SendMessage(chatId,
                "Active sessiya yo'q. /newsession bilan yarating.",
                cancellationToken: ct);
            return;
        }

        var dir = active.WorkingDirectory ?? "belgilanmagan";
        await bot.SendMessage(chatId,
            $"Sessiya: <b>{Esc(active.Name)}</b>\nFolder: <code>{Esc(dir)}</code>\nModel: <b>{active.Model}</b>",
            parseMode: ParseMode.Html, cancellationToken: ct);
    }

    private async Task HandleClear(
        ITelegramBotClient bot, long chatId, long userId, CancellationToken ct)
    {
        await _sessionStore.ClearSessionAsync(userId);

        await bot.SendMessage(chatId,
            "Context tozalandi. Yangi suhbat boshlandi.",
            cancellationToken: ct);
    }

    private async Task HandleModel(
        ITelegramBotClient bot, long chatId, long userId, string modelName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            await bot.SendMessage(chatId,
                "Foydalanish: /model &lt;name&gt;\n\nMavjud: <b>sonnet</b>, <b>opus</b>, <b>haiku</b>",
                parseMode: ParseMode.Html, cancellationToken: ct);
            return;
        }

        var model = modelName.ToLower().Trim();
        var validModels = new[] { "sonnet", "opus", "haiku" };

        if (!validModels.Contains(model))
        {
            await bot.SendMessage(chatId,
                $"Noto'g'ri model: <code>{Esc(model)}</code>\nMavjud: <b>sonnet</b>, <b>opus</b>, <b>haiku</b>",
                parseMode: ParseMode.Html, cancellationToken: ct);
            return;
        }

        var active = await _repo.GetActiveSession(userId);
        if (active != null)
            await _repo.UpdateSessionFields(active.Id, e => e.Model = model);

        await bot.SendMessage(chatId,
            $"Model o'zgartirildi: <b>{model}</b>",
            parseMode: ParseMode.Html, cancellationToken: ct);
    }

    private static string Esc(string text) =>
        text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
