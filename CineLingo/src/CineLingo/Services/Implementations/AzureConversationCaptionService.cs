using CineLingo.Configuration;
using CineLingo.Models;
using CineLingo.Services.Interfaces;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Transcription;

namespace CineLingo.Services.Implementations;

/// <summary>
/// Speech caption service for English content.
/// Uses Azure Conversation Transcription (S0 tier) to provide real punctuation
/// and speaker diarization (up to 10 speakers).
/// </summary>
public sealed class AzureConversationCaptionService : ISpeechCaptionService, IAsyncDisposable
{
    public event EventHandler<string>? PartialResultReceived;
    public event EventHandler<FinalResultEventArgs>? FinalResultReceived;
    public event EventHandler<string>? ErrorReceived;
#pragma warning disable CS0067 // StatusChanged is raised by SmartSpeechOrchestrator, not by this service directly
    public event EventHandler<string>? StatusChanged;
#pragma warning restore CS0067

    private ConversationTranscriber? _transcriber;
    private AudioConfig? _audioConfig;
    private bool _isRunning;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
            return;

        // Microphone permission is already granted by the time the orchestrator starts this
        // service, but we check again for safety in case this service runs standalone.
        var permStatus = await Permissions.RequestAsync<Permissions.Microphone>();
        if (permStatus != PermissionStatus.Granted)
        {
            ErrorReceived?.Invoke(this, "Microphone permission was not granted.");
            return;
        }

        var speechConfig = SpeechConfig.FromSubscription(
            AppConfiguration.SpeechSubscriptionKey,
            AppConfiguration.SpeechRegion);

        speechConfig.SetProfanity(ProfanityOption.Raw);

        _audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        _transcriber = new ConversationTranscriber(speechConfig, _audioConfig);

        _transcriber.Transcribing += OnTranscribing;
        _transcriber.Transcribed += OnTranscribed;
        _transcriber.Canceled += OnCanceled;
        _transcriber.SessionStopped += OnSessionStopped;

        await _transcriber.StartTranscribingAsync();
        _isRunning = true;
    }

    public async Task StopAsync()
    {
        if (!_isRunning || _transcriber is null)
            return;

        _isRunning = false;
        await _transcriber.StopTranscribingAsync();
        DisposeTranscriber();
    }

    private void OnTranscribing(object? sender, ConversationTranscriptionEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.Result.Text))
            PartialResultReceived?.Invoke(this, e.Result.Text);
    }

    private void OnTranscribed(object? sender, ConversationTranscriptionEventArgs e)
    {
        if (e.Result.Reason == ResultReason.RecognizedSpeech && !string.IsNullOrWhiteSpace(e.Result.Text))
        {
            var speakerId = FormatSpeakerId(e.Result.SpeakerId);
            FinalResultReceived?.Invoke(this, new FinalResultEventArgs(e.Result.Text, speakerId));
        }
    }

    private void OnCanceled(object? sender, ConversationTranscriptionCanceledEventArgs e)
    {
        _isRunning = false;
        if (e.Reason == CancellationReason.Error)
            ErrorReceived?.Invoke(this, $"Conversation recognition error ({e.ErrorCode}): {e.ErrorDetails}");
    }

    private void OnSessionStopped(object? sender, SessionEventArgs e)
    {
        _isRunning = false;
    }

    /// <summary>
    /// Maps the raw SDK speaker ID (e.g. "Guest-1") to a display label (e.g. "Speaker 1").
    /// Returns <c>null</c> for "Unknown" so the UI omits the speaker label.
    /// </summary>
    private static string? FormatSpeakerId(string? rawId)
    {
        if (string.IsNullOrEmpty(rawId) || rawId.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
            return null;

        // SDK returns "Guest-1", "Guest-2", etc.
        return rawId.StartsWith("Guest-", StringComparison.OrdinalIgnoreCase)
            ? "Speaker " + rawId["Guest-".Length..]
            : rawId;
    }

    private void DisposeTranscriber()
    {
        if (_transcriber is null) return;
        _transcriber.Transcribing -= OnTranscribing;
        _transcriber.Transcribed -= OnTranscribed;
        _transcriber.Canceled -= OnCanceled;
        _transcriber.SessionStopped -= OnSessionStopped;
        _transcriber.Dispose();
        _transcriber = null;
        _audioConfig?.Dispose();
        _audioConfig = null;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}
