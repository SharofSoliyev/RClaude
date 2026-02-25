using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Audio;
using RClaude.Configuration;

namespace RClaude.AudioProcessing;

/// <summary>
/// Service for transcribing audio messages using OpenAI Whisper API
/// </summary>
public class WhisperService
{
    private readonly OpenAIClient? _client;
    private readonly OpenAISettings _settings;
    private readonly ILogger<WhisperService> _logger;

    public WhisperService(
        IOptions<OpenAISettings> settings,
        ILogger<WhisperService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        if (!string.IsNullOrEmpty(_settings.ApiKey))
        {
            _client = new OpenAIClient(_settings.ApiKey);
        }
    }

    public bool IsAvailable => _client != null && _settings.EnableAudioProcessing;

    /// <summary>
    /// Transcribe audio file to text using Whisper API
    /// </summary>
    public async Task<string?> TranscribeAsync(string audioFilePath, CancellationToken ct = default)
    {
        if (_client == null)
        {
            _logger.LogWarning("OpenAI client not initialized. Check API key configuration.");
            return null;
        }

        try
        {
            _logger.LogInformation("Transcribing audio file: {FilePath}", audioFilePath);

            // Check if file exists and has content
            if (!File.Exists(audioFilePath))
            {
                _logger.LogError("Audio file not found: {FilePath}", audioFilePath);
                return null;
            }

            var fileInfo = new FileInfo(audioFilePath);
            _logger.LogInformation("Audio file size: {Size} bytes", fileInfo.Length);

            if (fileInfo.Length == 0)
            {
                _logger.LogError("Audio file is empty: {FilePath}", audioFilePath);
                return null;
            }

            var audioClient = _client.GetAudioClient("whisper-1");

            using var stream = File.OpenRead(audioFilePath);

            // Use proper filename with extension for Whisper API
            var fileName = Path.GetFileName(audioFilePath);

            _logger.LogInformation("Calling Whisper API with file: {FileName}", fileName);

            var result = await audioClient.TranscribeAudioAsync(stream, fileName,
                new AudioTranscriptionOptions
                {
                    // Don't specify language - let Whisper auto-detect for better accuracy
                    ResponseFormat = AudioTranscriptionFormat.Text,
                    Temperature = 0.0f // More deterministic
                });

            var transcription = result.Value.Text?.Trim();

            if (string.IsNullOrWhiteSpace(transcription))
            {
                _logger.LogWarning("Whisper returned empty transcription");
                return null;
            }

            _logger.LogInformation("Transcription successful: {Length} chars, text: {Preview}",
                transcription.Length,
                transcription.Length > 50 ? transcription.Substring(0, 50) + "..." : transcription);

            return transcription;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transcribing audio: {Message}", ex.Message);
            if (ex.InnerException != null)
            {
                _logger.LogError(ex.InnerException, "Inner exception: {Message}", ex.InnerException.Message);
            }
            return null;
        }
    }

    /// <summary>
    /// Check if audio duration is within limit
    /// </summary>
    public bool IsValidDuration(int durationSeconds)
    {
        return durationSeconds > 0 && durationSeconds <= _settings.MaxAudioDurationSeconds;
    }

    public int MaxDurationSeconds => _settings.MaxAudioDurationSeconds;
}
