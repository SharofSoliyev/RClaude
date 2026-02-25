namespace RClaude.Configuration;

/// <summary>
/// Bot UI matnlari — install vaqtida tanlangan tilda.
/// "uz" (default), "en", "ru"
/// </summary>
public class BotMessages
{
    // ─── Permission ─────────────────────────────────
    public string PermAllowed { get; init; } = "";
    public string PermDenied { get; init; } = "";
    public string PermBtnAllow { get; init; } = "";
    public string PermBtnDeny { get; init; } = "";
    public string PermWantsToUse { get; init; } = "";
    public string PermToolRunning { get; init; } = "";

    // ─── Auth ───────────────────────────────────────
    public string Unauthorized { get; init; } = "";
    public string UnknownCommand { get; init; } = "";

    // ─── Session ────────────────────────────────────
    public string SetDirFirst { get; init; } = "";
    public string RequestInProgress { get; init; } = "";
    public string Thinking { get; init; } = "";
    public string EmptyResponse { get; init; } = "";
    public string ErrorOccurred { get; init; } = "";
    public string NoWorkingDir { get; init; } = "";
    public string CliError { get; init; } = "";
    public string Timeout { get; init; } = "";
    public string GenericError { get; init; } = "";

    // ─── Commands ───────────────────────────────────
    public string StartMessage { get; init; } = "";
    public string HelpMessage { get; init; } = "";
    public string NoSessions { get; init; } = "";
    public string PickSession { get; init; } = "";
    public string SessionsTitle { get; init; } = "";
    public string FolderNotSet { get; init; } = "";
    public string NewSessionUsage { get; init; } = "";
    public string NewSessionCreated { get; init; } = "";
    public string SetDirNow { get; init; } = "";
    public string RenameUsage { get; init; } = "";
    public string NoActiveSession { get; init; } = "";
    public string SessionRenamed { get; init; } = "";
    public string NoSessionToDelete { get; init; } = "";
    public string SessionDeleted { get; init; } = "";
    public string CurrentSession { get; init; } = "";
    public string NoSessionsLeft { get; init; } = "";
    public string SetDirUsage { get; init; } = "";
    public string FolderNotFound { get; init; } = "";
    public string SessionSwitched { get; init; } = "";
    public string ContextCleared { get; init; } = "";
    public string ModelUsage { get; init; } = "";
    public string InvalidModel { get; init; } = "";
    public string ModelChanged { get; init; } = "";

    // ─── Factory ────────────────────────────────────
    public static BotMessages Create(string lang) => lang switch
    {
        "en" => English(),
        "ru" => Russian(),
        _ => Uzbek()
    };

    private static BotMessages Uzbek() => new()
    {
        PermAllowed = "✅ Ruxsat berildi",
        PermDenied = "❌ Rad etildi",
        PermBtnAllow = "✅ Ruxsat berish",
        PermBtnDeny = "❌ Rad etish",
        PermWantsToUse = "ishlatmoqchi",
        PermToolRunning = "ishlatilmoqda...",
        Unauthorized = "Ruxsat berilmagan. Administrator bilan bog'laning.",
        UnknownCommand = "Noma'lum buyruq. /help ni ko'ring.",
        SetDirFirst = "Avval folder belgilang:\n/setdir /path/to/project\n\nYoki yangi sessiya: /newsession &lt;nom&gt;",
        RequestInProgress = "Oldingi so'rov hali bajarilmoqda. Iltimos kuting...",
        Thinking = "O'ylamoqda...",
        EmptyResponse = "(bo'sh javob)",
        ErrorOccurred = "Xatolik yuz berdi",
        NoWorkingDir = "Working directory belgilanmagan. /setdir buyrug'ini ishlating.",
        CliError = "Claude CLI xatosi",
        Timeout = "So'rov vaqti tugadi",
        GenericError = "Xatolik",
        StartMessage = """
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
            """,
        HelpMessage = """
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
            """,
        NoSessions = "Sessiyalar yo'q. /newsession &lt;nom&gt; bilan yarating.",
        PickSession = "Sessiya tanlang:",
        SessionsTitle = "Sessiyalar:",
        FolderNotSet = "folder belgilanmagan",
        NewSessionUsage = "Foydalanish: /newsession &lt;nom&gt;\nMasalan: /newsession Wordy Backend",
        NewSessionCreated = "Yangi sessiya yaratildi",
        SetDirNow = "Endi /setdir bilan folder belgilang.",
        RenameUsage = "Foydalanish: /renamesession &lt;yangi nom&gt;",
        NoActiveSession = "Active sessiya yo'q.",
        SessionRenamed = "Sessiya nomi o'zgartirildi",
        NoSessionToDelete = "O'chirish uchun sessiya yo'q.",
        SessionDeleted = "Sessiya o'chirildi",
        CurrentSession = "Hozirgi sessiya",
        NoSessionsLeft = "Sessiyalar qolmadi. /newsession bilan yarating.",
        SetDirUsage = "Foydalanish: /setdir /path/to/project",
        FolderNotFound = "Papka topilmadi",
        SessionSwitched = "Sessiya almashtirildi",
        ContextCleared = "Context tozalandi. Yangi suhbat boshlandi.",
        ModelUsage = "Foydalanish: /model &lt;name&gt;\n\nMavjud: <b>sonnet</b>, <b>opus</b>, <b>haiku</b>",
        InvalidModel = "Noto'g'ri model",
        ModelChanged = "Model o'zgartirildi"
    };

    private static BotMessages English() => new()
    {
        PermAllowed = "✅ Allowed",
        PermDenied = "❌ Denied",
        PermBtnAllow = "✅ Allow",
        PermBtnDeny = "❌ Deny",
        PermWantsToUse = "wants to use",
        PermToolRunning = "running...",
        Unauthorized = "Access denied. Contact the administrator.",
        UnknownCommand = "Unknown command. See /help.",
        SetDirFirst = "Set a folder first:\n/setdir /path/to/project\n\nOr create a session: /newsession &lt;name&gt;",
        RequestInProgress = "Previous request is still running. Please wait...",
        Thinking = "Thinking...",
        EmptyResponse = "(empty response)",
        ErrorOccurred = "Error occurred",
        NoWorkingDir = "Working directory not set. Use /setdir command.",
        CliError = "Claude CLI error",
        Timeout = "Request timed out",
        GenericError = "Error",
        StartMessage = """
            <b>RClaude — Claude Code Agent</b>

            Code assistant via Telegram!
            Works just like the VS Code extension.

            <b>Session commands:</b>
            /session — Pick session (inline keyboard)
            /sessions — List all sessions
            /newsession &lt;name&gt; — Create new session
            /renamesession &lt;name&gt; — Rename session
            /deletesession — Delete current session

            <b>Work commands:</b>
            /setdir &lt;path&gt; — Set folder
            /getdir — Current info
            /clear — Clear conversation
            /model &lt;name&gt; — Model (sonnet/opus/haiku)
            /help — Help

            Start: /newsession MyProject → /setdir /path → send a message!
            """,
        HelpMessage = """
            <b>RClaude Commands:</b>

            <b>Sessions:</b>
            /session — Pick session (button)
            /sessions — List all sessions
            /newsession &lt;name&gt; — New session
            /renamesession &lt;name&gt; — Rename
            /deletesession — Delete session

            <b>Work:</b>
            /setdir &lt;path&gt; — Set folder
            /getdir — Current session info
            /clear — Clear context
            /model &lt;name&gt; — sonnet, opus, haiku

            <b>How it works:</b>
            Send a message — Claude Code reads, writes, edits files and runs shell commands.
            """,
        NoSessions = "No sessions. Create one with /newsession &lt;name&gt;.",
        PickSession = "Pick a session:",
        SessionsTitle = "Sessions:",
        FolderNotSet = "folder not set",
        NewSessionUsage = "Usage: /newsession &lt;name&gt;\nExample: /newsession Wordy Backend",
        NewSessionCreated = "New session created",
        SetDirNow = "Now set a folder with /setdir.",
        RenameUsage = "Usage: /renamesession &lt;new name&gt;",
        NoActiveSession = "No active session.",
        SessionRenamed = "Session renamed",
        NoSessionToDelete = "No session to delete.",
        SessionDeleted = "Session deleted",
        CurrentSession = "Current session",
        NoSessionsLeft = "No sessions left. Create one with /newsession.",
        SetDirUsage = "Usage: /setdir /path/to/project",
        FolderNotFound = "Folder not found",
        SessionSwitched = "Session switched",
        ContextCleared = "Context cleared. New conversation started.",
        ModelUsage = "Usage: /model &lt;name&gt;\n\nAvailable: <b>sonnet</b>, <b>opus</b>, <b>haiku</b>",
        InvalidModel = "Invalid model",
        ModelChanged = "Model changed"
    };

    private static BotMessages Russian() => new()
    {
        PermAllowed = "✅ Разрешено",
        PermDenied = "❌ Отклонено",
        PermBtnAllow = "✅ Разрешить",
        PermBtnDeny = "❌ Отклонить",
        PermWantsToUse = "хочет использовать",
        PermToolRunning = "выполняется...",
        Unauthorized = "Доступ запрещён. Обратитесь к администратору.",
        UnknownCommand = "Неизвестная команда. Смотрите /help.",
        SetDirFirst = "Сначала укажите папку:\n/setdir /path/to/project\n\nИли создайте сессию: /newsession &lt;имя&gt;",
        RequestInProgress = "Предыдущий запрос ещё выполняется. Подождите...",
        Thinking = "Думаю...",
        EmptyResponse = "(пустой ответ)",
        ErrorOccurred = "Произошла ошибка",
        NoWorkingDir = "Рабочая директория не указана. Используйте /setdir.",
        CliError = "Ошибка Claude CLI",
        Timeout = "Время ожидания истекло",
        GenericError = "Ошибка",
        StartMessage = """
            <b>RClaude — Claude Code Agent</b>

            Помощник по коду через Telegram!
            Работает как расширение VS Code.

            <b>Команды сессий:</b>
            /session — Выбрать сессию (inline keyboard)
            /sessions — Список всех сессий
            /newsession &lt;имя&gt; — Создать сессию
            /renamesession &lt;имя&gt; — Переименовать сессию
            /deletesession — Удалить текущую сессию

            <b>Рабочие команды:</b>
            /setdir &lt;path&gt; — Указать папку
            /getdir — Текущая информация
            /clear — Очистить диалог
            /model &lt;name&gt; — Модель (sonnet/opus/haiku)
            /help — Помощь

            Начать: /newsession MyProject → /setdir /path → отправьте сообщение!
            """,
        HelpMessage = """
            <b>Команды RClaude:</b>

            <b>Сессии:</b>
            /session — Выбрать сессию (кнопка)
            /sessions — Список всех сессий
            /newsession &lt;имя&gt; — Новая сессия
            /renamesession &lt;имя&gt; — Переименовать
            /deletesession — Удалить сессию

            <b>Работа:</b>
            /setdir &lt;path&gt; — Указать папку
            /getdir — Информация о текущей сессии
            /clear — Очистить контекст
            /model &lt;name&gt; — sonnet, opus, haiku

            <b>Как это работает:</b>
            Отправьте сообщение — Claude Code читает, пишет, редактирует файлы и выполняет команды.
            """,
        NoSessions = "Нет сессий. Создайте: /newsession &lt;имя&gt;.",
        PickSession = "Выберите сессию:",
        SessionsTitle = "Сессии:",
        FolderNotSet = "папка не указана",
        NewSessionUsage = "Использование: /newsession &lt;имя&gt;\nПример: /newsession Wordy Backend",
        NewSessionCreated = "Новая сессия создана",
        SetDirNow = "Теперь укажите папку с /setdir.",
        RenameUsage = "Использование: /renamesession &lt;новое имя&gt;",
        NoActiveSession = "Нет активной сессии.",
        SessionRenamed = "Сессия переименована",
        NoSessionToDelete = "Нет сессии для удаления.",
        SessionDeleted = "Сессия удалена",
        CurrentSession = "Текущая сессия",
        NoSessionsLeft = "Сессий не осталось. Создайте: /newsession.",
        SetDirUsage = "Использование: /setdir /path/to/project",
        FolderNotFound = "Папка не найдена",
        SessionSwitched = "Сессия переключена",
        ContextCleared = "Контекст очищен. Новый диалог начат.",
        ModelUsage = "Использование: /model &lt;name&gt;\n\nДоступны: <b>sonnet</b>, <b>opus</b>, <b>haiku</b>",
        InvalidModel = "Неверная модель",
        ModelChanged = "Модель изменена"
    };
}
