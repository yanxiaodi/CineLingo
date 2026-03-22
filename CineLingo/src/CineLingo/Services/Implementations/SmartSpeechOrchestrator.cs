using CineLingo.Models;
using CineLingo.Services.Interfaces;

namespace CineLingo.Services.Implementations;

/// <summary>
/// Orchestrates dual-mode captioning:
/// 1. Starts in multi-language detection mode (<see cref="AzureSpeechCaptionService"/>).
/// 2. On the first finalized utterance, inspects the detected language.
///    - English → stops multi-language mode and switches to <see cref="AzureConversationCaptionService"/>
///      for real punctuation and speaker diarization.
///    - Other language → keeps multi-language mode running; no diarization.
/// If the Conversation Speech credentials are not configured, the orchestrator
/// stays in multi-language mode regardless of detected language.
/// </summary>
public sealed class SmartSpeechOrchestrator : ISpeechCaptionService, IAsyncDisposable
{
    public event EventHandler<string>? PartialResultReceived;
    public event EventHandler<FinalResultEventArgs>? FinalResultReceived;
    public event EventHandler<string>? ErrorReceived;
    public event EventHandler<string>? StatusChanged;

    private readonly AzureSpeechCaptionService _multiLangService;
    private readonly AzureConversationCaptionService _conversationService;

    private bool _detectionDone;
    private bool _isRunning;

    public SmartSpeechOrchestrator(
        AzureSpeechCaptionService multiLangService,
        AzureConversationCaptionService conversationService)
    {
        _multiLangService = multiLangService;
        _conversationService = conversationService;

        _multiLangService.PartialResultReceived += OnPartialResult;
        _multiLangService.FinalResultReceived += OnMultiLangFinalResult;
        _multiLangService.ErrorReceived += OnError;

        _conversationService.PartialResultReceived += OnPartialResult;
        _conversationService.FinalResultReceived += OnConversationFinalResult;
        _conversationService.ErrorReceived += OnError;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning) return;

        _isRunning = true;
        _detectionDone = false;
        StatusChanged?.Invoke(this, "Detecting language...");
        await _multiLangService.StartAsync(cancellationToken);
    }

    public async Task StopAsync()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _detectionDone = false;
        await _multiLangService.StopAsync();
        await _conversationService.StopAsync();
        StatusChanged?.Invoke(this, string.Empty);
    }

    private void OnMultiLangFinalResult(object? sender, FinalResultEventArgs e)
    {
        // Forward every result from the multi-language recognizer to the UI.
        FinalResultReceived?.Invoke(this, e);

        if (_detectionDone) return;
        _detectionDone = true;

        var detected = e.DetectedLanguage;

        // Only switch to conversation mode if English is detected AND credentials are configured.
        if (detected == "en-US")
        {
            Task.Run(async () =>
            {
                StatusChanged?.Invoke(this, "Switching to English mode...");
                await _multiLangService.StopAsync();
                await _conversationService.StartAsync();
                StatusChanged?.Invoke(this, "English · Speaker diarization enabled");
            });
        }
        else if (detected != null)
        {
            var langDisplay = detected.Split('-')[0].ToUpperInvariant();
            StatusChanged?.Invoke(this, $"Multi-language mode ({langDisplay})");
        }
    }

    private void OnConversationFinalResult(object? sender, FinalResultEventArgs e)
    {
        FinalResultReceived?.Invoke(this, e);
    }

    private void OnPartialResult(object? sender, string text)
    {
        PartialResultReceived?.Invoke(this, text);
    }

    private void OnError(object? sender, string message)
    {
        ErrorReceived?.Invoke(this, message);
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        await _multiLangService.DisposeAsync();
        await _conversationService.DisposeAsync();
    }
}
