using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RClaude.Claude;
using RClaude.Configuration;
using RClaude.Data;
using RClaude.Permission;
using RClaude.Session;
using RClaude.Telegram;

// --init-db mode: installer skriptlari uchun DB yaratish va sozlamalar yozish
// sqlite3 CLI kerak emas — .NET o'zi bajaradi
if (args.Contains("--init-db"))
{
    await InitializeDatabase(args);
    return;
}

var builder = Host.CreateApplicationBuilder(args);

// Database — SQLite or PostgreSQL based on config
var dbProvider = builder.Configuration["Database:Provider"]?.ToLower() ?? "sqlite";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    if (dbProvider == "postgresql")
        options.UseNpgsql(connectionString);
    else
        options.UseSqlite(connectionString ?? "Data Source=rclaude.db");
});

// Ensure database is created and load settings from DB
// We need to build a temporary service provider to access the DB
var tempProvider = builder.Services.BuildServiceProvider();
var dbFactory = tempProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
await using (var db = await dbFactory.CreateDbContextAsync())
{
    await db.Database.EnsureCreatedAsync();
}

// Load settings from DB
var settingsRepo = new SettingsRepository(dbFactory);
var allSettings = await settingsRepo.GetAllAsync();

// Configure services from DB settings
builder.Services.Configure<TelegramSettings>(opts =>
{
    opts.BotToken = allSettings.GetValueOrDefault("telegram:bot_token", "");

    var usernames = allSettings.GetValueOrDefault("telegram:allowed_usernames", "[]");
    opts.AllowedUsernames = JsonSerializer.Deserialize<string[]>(usernames) ?? [];

    var userIds = allSettings.GetValueOrDefault("telegram:allowed_user_ids", "[]");
    opts.AllowedUserIds = JsonSerializer.Deserialize<long[]>(userIds) ?? [];
});

builder.Services.Configure<ClaudeSettings>(opts =>
{
    opts.CliBinaryPath = allSettings.GetValueOrDefault("claude:cli_path", "claude");
    opts.Model = allSettings.GetValueOrDefault("claude:model", "sonnet");
    opts.PermissionMode = allSettings.GetValueOrDefault("claude:permission_mode", "ask");

    if (int.TryParse(allSettings.GetValueOrDefault("claude:max_timeout", "600"), out var timeout))
        opts.MaxTimeoutSeconds = timeout;
});

builder.Services.Configure<AgentSettings>(opts =>
{
    opts.DefaultWorkingDirectory = allSettings.GetValueOrDefault("agent:default_dir", "");
});

// i18n — Bot messages in selected language
var botLang = allSettings.GetValueOrDefault("bot:language", "uz");
builder.Services.AddSingleton(BotMessages.Create(botLang));

// Data
builder.Services.AddSingleton<SettingsRepository>();
builder.Services.AddSingleton<SessionRepository>();

// Session
builder.Services.AddSingleton<SessionStore>();

// Permission
builder.Services.AddSingleton<PermissionService>();

// Claude
builder.Services.AddSingleton<ClaudeCliService>();

// Telegram
builder.Services.AddSingleton<MessageFormatter>();
builder.Services.AddSingleton<CommandHandler>();
builder.Services.AddSingleton<UpdateHandler>();
builder.Services.AddHostedService<TelegramHostedService>();

var host = builder.Build();

// Start permission service and ensure hook script exists
var permissionService = host.Services.GetRequiredService<PermissionService>();
permissionService.Start();
PermissionHookSetup.EnsureHookScript();

host.Run();

// ─── --init-db: DB yaratish va sozlamalar yozish ───
static async Task InitializeDatabase(string[] args)
{
    string GetArg(string name)
    {
        var idx = Array.IndexOf(args, name);
        return idx >= 0 && idx + 1 < args.Length ? args[idx + 1] : "";
    }

    var installDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".rclaude");
    var dbPath = Path.Combine(installDir, "rclaude.db");

    Directory.CreateDirectory(installDir);

    var connectionString = $"Data Source={dbPath}";
    var services = new ServiceCollection();
    services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connectionString));
    var provider = services.BuildServiceProvider();
    var factory = provider.GetRequiredService<IDbContextFactory<AppDbContext>>();

    await using (var db = await factory.CreateDbContextAsync())
    {
        await db.Database.EnsureCreatedAsync();
    }

    var repo = new SettingsRepository(factory);

    if (args.Contains("--update-claude-path"))
    {
        // Upgrade rejim — faqat Claude CLI yo'lini yangilash
        var claudePath = GetArg("--claude-path");
        if (!string.IsNullOrEmpty(claudePath))
            await repo.SetAsync("claude:cli_path", claudePath);
    }
    else
    {
        // To'liq o'rnatish
        var botToken = GetArg("--bot-token");
        var username = GetArg("--username");
        var claudePath = GetArg("--claude-path");
        var permissionMode = GetArg("--permission-mode");

        if (string.IsNullOrEmpty(botToken))
        {
            Console.Error.WriteLine("ERROR: --bot-token is required");
            Environment.Exit(1);
        }
        await repo.SetAsync("telegram:bot_token", botToken);

        if (string.IsNullOrEmpty(username))
        {
            Console.Error.WriteLine("ERROR: --username is required");
            Environment.Exit(1);
        }
        await repo.SetAsync("telegram:allowed_usernames", $"[\"{username}\"]");
        await repo.SetAsync("telegram:allowed_user_ids", "[]");

        if (!string.IsNullOrEmpty(claudePath))
            await repo.SetAsync("claude:cli_path", claudePath);

        await repo.SetAsync("claude:model", "sonnet");
        await repo.SetAsync("claude:max_timeout", "600");
        await repo.SetAsync("claude:permission_mode",
            string.IsNullOrEmpty(permissionMode) ? "ask" : permissionMode);

        var language = GetArg("--language");
        await repo.SetAsync("bot:language",
            string.IsNullOrEmpty(language) ? "uz" : language);
    }

    Console.WriteLine("Database initialized successfully.");
}
