using CineLingo.Configuration;
using CineLingo.Models;
using CineLingo.Services.Interfaces;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace CineLingo.Services.Implementations;

public sealed class AzureSpeechCaptionService : ISpeechCaptionService, IAsyncDisposable
{
    public event EventHandler<string>? PartialResultReceived;
    public event EventHandler<FinalResultEventArgs>? FinalResultReceived;
    public event EventHandler<string>? ErrorReceived;
#pragma warning disable CS0067 // StatusChanged is raised by SmartSpeechOrchestrator, not by this service directly
    public event EventHandler<string>? StatusChanged;
#pragma warning restore CS0067

    private SpeechRecognizer? _recognizer;
    private AudioConfig? _audioConfig;
    private bool _isRunning;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
            return;

        var status = await Permissions.RequestAsync<Permissions.Microphone>();
        if (status != PermissionStatus.Granted)
        {
            ErrorReceived?.Invoke(this, "Microphone permission was not granted.");
            return;
        }

        var speechConfig = SpeechConfig.FromEndpoint(
            new Uri(AppConfiguration.SpeechEndpoint),
            AppConfiguration.SpeechSubscriptionKey);

        // Auto-detect language — max 4 candidates supported by Azure Speech SDK.
        var autoDetectConfig = AutoDetectSourceLanguageConfig.FromLanguages(
            ["en-US", "zh-CN", "ja-JP", "ko-KR"]);

        speechConfig.OutputFormat = OutputFormat.Simple;
        speechConfig.SetProfanity(ProfanityOption.Raw);
        speechConfig.SetServiceProperty("punctuation", "explicit", ServicePropertyChannel.UriQueryParameter);

        _audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        _recognizer = new SpeechRecognizer(speechConfig, autoDetectConfig, _audioConfig);

        _recognizer.Recognizing += OnRecognizing;
        _recognizer.Recognized += OnRecognized;
        _recognizer.Canceled += OnCanceled;
        _recognizer.SessionStopped += OnSessionStopped;

        await _recognizer.StartContinuousRecognitionAsync();
        _isRunning = true;
    }

    public async Task StopAsync()
    {
        if (!_isRunning || _recognizer is null)
            return;

        _isRunning = false;
        await _recognizer.StopContinuousRecognitionAsync();
        DisposeRecognizer();
    }

    private void OnRecognizing(object? sender, SpeechRecognitionEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.Result.Text))
            PartialResultReceived?.Invoke(this, e.Result.Text);
    }

    private void OnRecognized(object? sender, SpeechRecognitionEventArgs e)
    {
        if (e.Result.Reason == ResultReason.RecognizedSpeech && !string.IsNullOrWhiteSpace(e.Result.Text))
        {
            var langResult = AutoDetectSourceLanguageResult.FromResult(e.Result);
            FinalResultReceived?.Invoke(this, new FinalResultEventArgs(e.Result.Text, null, langResult?.Language));
        }
    }

    private void OnCanceled(object? sender, SpeechRecognitionCanceledEventArgs e)
    {
        _isRunning = false;
        if (e.Reason == CancellationReason.Error)
            ErrorReceived?.Invoke(this, $"Recognition error ({e.ErrorCode}): {e.ErrorDetails}");
    }

    private void OnSessionStopped(object? sender, SessionEventArgs e)
    {
        _isRunning = false;
    }

    private void DisposeRecognizer()
    {
        if (_recognizer is null) return;
        _recognizer.Recognizing -= OnRecognizing;
        _recognizer.Recognized -= OnRecognized;
        _recognizer.Canceled -= OnCanceled;
        _recognizer.SessionStopped -= OnSessionStopped;
        _recognizer.Dispose();
        _recognizer = null;
        _audioConfig?.Dispose();
        _audioConfig = null;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}
