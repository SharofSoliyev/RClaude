using Microsoft.Extensions.Options;
using RClaude.Configuration;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RClaude.Telegram;

public class TelegramHostedService : BackgroundService
{
    private readonly UpdateHandler _handler;
    private readonly TelegramSettings _settings;
    private readonly ILogger<TelegramHostedService> _logger;

    public TelegramHostedService(
        UpdateHandler handler,
        IOptions<TelegramSettings> settings,
        ILogger<TelegramHostedService> logger)
    {
        _handler = handler;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.BotToken)
            || _settings.BotToken == "YOUR_BOT_TOKEN_HERE")
        {
            _logger.LogError("Telegram bot token belgilanmagan! appsettings.json ni tekshiring.");
            return;
        }

        var bot = new TelegramBotClient(_settings.BotToken);

        try
        {
            var me = await bot.GetMe(stoppingToken);
            _logger.LogInformation(
                "RClaude bot ishga tushdi: @{Username} (ID: {Id})",
                me.Username, me.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bot ga ulanib bo'lmadi. Token ni tekshiring.");
            return;
        }

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
            DropPendingUpdates = true
        };

        bot.StartReceiving(
            updateHandler: async (client, update, ct) =>
            {
                try
                {
                    await _handler.HandleUpdateAsync(client, update, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Update handling xatosi");
                }
            },
            errorHandler: async (client, exception, source, ct) =>
            {
                _logger.LogError(exception, "Polling xatosi: {Source}", source);
                await Task.Delay(3000, ct);
            },
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken
        );

        _logger.LogInformation("Bot xabarlarni kutmoqda...");

        // Keep service running
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
