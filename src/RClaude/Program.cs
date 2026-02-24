using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RClaude.Claude;
using RClaude.Configuration;
using RClaude.Data;
using RClaude.Session;
using RClaude.Telegram;

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

    if (int.TryParse(allSettings.GetValueOrDefault("claude:max_timeout", "600"), out var timeout))
        opts.MaxTimeoutSeconds = timeout;
});

builder.Services.Configure<AgentSettings>(opts =>
{
    opts.DefaultWorkingDirectory = allSettings.GetValueOrDefault("agent:default_dir", "");
});

// Data
builder.Services.AddSingleton<SettingsRepository>();
builder.Services.AddSingleton<SessionRepository>();

// Session
builder.Services.AddSingleton<SessionStore>();

// Claude
builder.Services.AddSingleton<ClaudeCliService>();

// Telegram
builder.Services.AddSingleton<MessageFormatter>();
builder.Services.AddSingleton<CommandHandler>();
builder.Services.AddSingleton<UpdateHandler>();
builder.Services.AddHostedService<TelegramHostedService>();

var host = builder.Build();
host.Run();
