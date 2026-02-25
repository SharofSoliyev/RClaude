using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using RClaude.Configuration;

namespace RClaude.AudioProcessing;

/// <summary>
/// Service for optimizing user prompts using GPT models
/// </summary>
public class PromptOptimizer
{
    private readonly OpenAIClient? _client;
    private readonly OpenAISettings _settings;
    private readonly ILogger<PromptOptimizer> _logger;

    private const string SystemPrompt = @"Sen professional prompt optimizator AI assistantsan.

Foydalanuvchi sening yordaming bilan o'z promptini Claude Code AI agentiga yubormoqchi. Sening vazifang:

1. Foydalanuvchi promptini tahlil qilish
2. Promptni aniqroq, tushunarli va samaraliroq qilish
3. Agar kerak bo'lsa, kontekst qo'shish va yaxshilash
4. Grammatik xatolarni tuzatish

MUHIM QOIDALAR:
- Foydalanuvchining asosiy maqsadini saqla
- Ortiqcha so'zlarni qo'shma
- Qisqa va aniq bo'lsin
- Agar prompt allaqachon yaxshi bo'lsa, faqat ozgina yaxshilash
- Javobda FAQAT optimizatsiya qilingan promptni yoz, boshqa hech narsa yo'q

Misol:
Input: ""iltimos menig kodimni tekshir va xatolarni topib ber""
Output: ""Kodni tekshirib, xatolarni aniqlang va tuzatish bo'yicha tavsiyalar bering.""

Input: ""websaytim uchun login qismi qilish kerak""
Output: ""Web sayt uchun foydalanuvchi login sahifasi yarating. Username va password input fieldlari, login button va validatsiya qo'shing.""";

    public PromptOptimizer(
        IOptions<OpenAISettings> settings,
        ILogger<PromptOptimizer> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        if (!string.IsNullOrEmpty(_settings.ApiKey))
        {
            _client = new OpenAIClient(_settings.ApiKey);
        }
    }

    public bool IsAvailable => _client != null;

    /// <summary>
    /// Optimize user prompt using GPT
    /// </summary>
    public async Task<string?> OptimizeAsync(string originalPrompt, CancellationToken ct = default)
    {
        if (_client == null)
        {
            _logger.LogWarning("OpenAI client not initialized");
            return originalPrompt; // Return original if optimization unavailable
        }

        try
        {
            _logger.LogInformation("Optimizing prompt: {Length} chars", originalPrompt.Length);

            var chatClient = _client.GetChatClient(_settings.PromptOptimizationModel);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(SystemPrompt),
                new UserChatMessage(originalPrompt)
            };

            var completion = await chatClient.CompleteChatAsync(messages,
                new ChatCompletionOptions
                {
                    Temperature = 0.3f, // Low temperature for consistency
                    MaxOutputTokenCount = 500, // Limit output length
                }, ct);

            var optimized = completion.Value.Content[0].Text?.Trim();

            if (string.IsNullOrWhiteSpace(optimized))
            {
                _logger.LogWarning("GPT returned empty optimization");
                return originalPrompt;
            }

            _logger.LogInformation("Optimization successful: {OriginalLength} -> {OptimizedLength} chars",
                originalPrompt.Length, optimized.Length);

            return optimized;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing prompt");
            return originalPrompt; // Return original on error
        }
    }
}
