namespace CineLingo.Models;

/// <summary>
/// Carries the outcome of a single finalized speech recognition utterance.
/// </summary>
public sealed class FinalResultEventArgs : EventArgs
{
    /// <summary>Transcribed text of the utterance.</summary>
    public string Text { get; }

    /// <summary>
    /// Human-readable speaker label (e.g. "Speaker 1") when diarization is active;
    /// <c>null</c> in multi-language mode where diarization is not available.
    /// </summary>
    public string? SpeakerId { get; }

    /// <summary>
    /// BCP-47 language tag detected by AutoDetect (e.g. "en-US", "zh-CN");
    /// <c>null</c> when running in conversation/diarization mode where language is fixed.
    /// </summary>
    public string? DetectedLanguage { get; }

    public FinalResultEventArgs(string text, string? speakerId = null, string? detectedLanguage = null)
    {
        Text = text;
        SpeakerId = speakerId;
        DetectedLanguage = detectedLanguage;
    }
}
