using CineLingo.Models;

namespace CineLingo.Services.Interfaces;

public interface ISpeechCaptionService
{
    /// <summary>Fired when a partial/interim transcription result is available (speech still in progress).</summary>
    event EventHandler<string>? PartialResultReceived;

    /// <summary>Fired when a finalized transcription result is available (utterance complete).</summary>
    event EventHandler<FinalResultEventArgs>? FinalResultReceived;

    /// <summary>Fired when a recognition error or session cancellation occurs.</summary>
    event EventHandler<string>? ErrorReceived;

    /// <summary>Fired when the active recognition mode changes (e.g. detecting, English diarization, multi-language).</summary>
    event EventHandler<string>? StatusChanged;

    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync();
}
