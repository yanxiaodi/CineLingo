namespace CineLingo.Models;

/// <summary>
/// User preferences for the translation feature, persisted via MAUI Preferences.
/// </summary>
public sealed class TranslationSettings
{
    private const string TargetLangKey = "trans_target_lang";
    private const string IsEnabledKey  = "trans_enabled";

    /// <summary>
    /// Translator language code for the output language (e.g. "zh-Hans", "en", "ja").
    /// Defaults to Simplified Chinese.
    /// </summary>
    public string TargetLanguage
    {
        get => Preferences.Default.Get(TargetLangKey, "zh-Hans");
        set => Preferences.Default.Set(TargetLangKey, value);
    }

    /// <summary>Whether automatic translation is switched on.</summary>
    public bool IsEnabled
    {
        get => Preferences.Default.Get(IsEnabledKey, true);
        set => Preferences.Default.Set(IsEnabledKey, value);
    }
}
