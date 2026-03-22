using CommunityToolkit.Mvvm.ComponentModel;

namespace CineLingo.Models;

public partial class Caption : ObservableObject
{
    [ObservableProperty]
    public partial string Text { get; set; } = string.Empty;

    /// <summary>
    /// Translated text; set asynchronously after the caption is added to the list.
    /// Triggers updates to <see cref="DisplayText"/> and <see cref="HasTranslation"/>.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    [NotifyPropertyChangedFor(nameof(HasTranslation))]
    public partial string? TranslatedText { get; set; }

    /// <summary>Returns translated text when available, original text otherwise.</summary>
    public string DisplayText => TranslatedText ?? Text;

    /// <summary>True once a translation has arrived; used to show the original text as a subtitle.</summary>
    public bool HasTranslation => !string.IsNullOrEmpty(TranslatedText);

    public Color TextColor { get; init; } = Colors.WhiteSmoke;

    /// <summary>
    /// Display label for the speaker (e.g. "Speaker 1").
    /// <c>null</c> or empty in multi-language mode where diarization is unavailable.
    /// </summary>
    public string? SpeakerLabel { get; init; }

    /// <summary>True when a speaker label is available; used for compiled-binding IsVisible.</summary>
    public bool HasSpeakerLabel => !string.IsNullOrEmpty(SpeakerLabel);

    public Caption() { }

    public Caption(string text, Color textColor, string? speakerLabel = null)
    {
        Text = text;
        TextColor = textColor;
        SpeakerLabel = speakerLabel;
    }
}
