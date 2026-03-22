namespace CineLingo.Models;

/// <summary>All user-configurable preferences, persisted via MAUI Preferences.</summary>
public sealed class AppSettings
{
    // ── Translation ───────────────────────────────────────────────────────
    public string TargetLanguage
    {
        get => Preferences.Default.Get("trans_target_lang", "zh-Hans");
        set => Preferences.Default.Set("trans_target_lang", value);
    }

    public bool IsTranslationEnabled
    {
        get => Preferences.Default.Get("trans_enabled", true);
        set => Preferences.Default.Set("trans_enabled", value);
    }

    // ── Display ───────────────────────────────────────────────────────────
    /// <summary>Caption font size. Stored value is one of: 14, 18, 22, 26.</summary>
    public double CaptionFontSize
    {
        get => Preferences.Default.Get("caption_font_size", 18.0);
        set => Preferences.Default.Set("caption_font_size", value);
    }

    /// <summary>Max finalized captions to keep in the list. 0 = unlimited.</summary>
    public int MaxCaptionCount
    {
        get => Preferences.Default.Get("max_captions", 100);
        set => Preferences.Default.Set("max_captions", value);
    }

    // ── Speech ────────────────────────────────────────────────────────────
    /// <summary>When true, switches to speaker diarization mode if English is detected.</summary>
    public bool EnableSpeakerDiarization
    {
        get => Preferences.Default.Get("enable_diarization", true);
        set => Preferences.Default.Set("enable_diarization", value);
    }
}
