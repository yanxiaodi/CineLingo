using CineLingo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CineLingo.PageModels;

public partial class SettingsPageModel : ObservableObject
{
    public record LanguageOption(string Code, string DisplayName);

    public static readonly IReadOnlyList<LanguageOption> SupportedLanguages =
    [
        new("zh-Hans", "简体中文 (Simplified Chinese)"),
        new("zh-Hant", "繁體中文 (Traditional Chinese)"),
        new("en",      "English"),
        new("ja",      "日本語 (Japanese)"),
        new("ko",      "한국어 (Korean)"),
        new("fr",      "Français (French)"),
        new("de",      "Deutsch (German)"),
        new("es",      "Español (Spanish)"),
        new("pt",      "Português (Portuguese)"),
        new("ru",      "Русский (Russian)"),
        new("it",      "Italiano (Italian)"),
        new("ar",      "العربية (Arabic)"),
    ];

    private readonly AppSettings _settings;

    [ObservableProperty] public partial bool           IsTranslationEnabled     { get; set; }
    [ObservableProperty] public partial LanguageOption SelectedLanguage         { get; set; }
    [ObservableProperty] public partial bool           EnableSpeakerDiarization { get; set; }

    /// <summary>Font size slider value (12–30). Drives DynamicResource immediately.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FontSizeDisplayText))]
    public partial double CaptionFontSize { get; set; }

    /// <summary>Slider value for max captions (0–500). 0 means Unlimited.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MaxCaptionDisplayText))]
    public partial double MaxCaptionSliderValue { get; set; }

    public string FontSizeDisplayText    => $"{(int)Math.Round(CaptionFontSize)}px";
    public string MaxCaptionDisplayText  => MaxCaptionSliderValue <= 5 ? "Unlimited"
                                            : $"{(int)(Math.Round(MaxCaptionSliderValue / 10.0) * 10)}";

    public SettingsPageModel(AppSettings settings)
    {
        _settings = settings;

        IsTranslationEnabled     = settings.IsTranslationEnabled;
        SelectedLanguage         = SupportedLanguages.FirstOrDefault(l => l.Code == settings.TargetLanguage)
                                   ?? SupportedLanguages[0];
        CaptionFontSize          = settings.CaptionFontSize;
        MaxCaptionSliderValue    = settings.MaxCaptionCount <= 0 ? 0 : settings.MaxCaptionCount;
        EnableSpeakerDiarization = settings.EnableSpeakerDiarization;
    }

    partial void OnIsTranslationEnabledChanged(bool value)         => _settings.IsTranslationEnabled = value;
    partial void OnSelectedLanguageChanged(LanguageOption value)  { if (value != null) _settings.TargetLanguage = value.Code; }
    partial void OnEnableSpeakerDiarizationChanged(bool value)     => _settings.EnableSpeakerDiarization = value;

    partial void OnCaptionFontSizeChanged(double value)
    {
        _settings.CaptionFontSize = value;
        // Update DynamicResource so captions on MainPage resize immediately.
        if (Application.Current?.Resources is not null)
            Application.Current.Resources["CaptionFontSize"] = value;
    }

    partial void OnMaxCaptionSliderValueChanged(double value)
    {
        // Round to nearest 10; treat ≤5 as Unlimited (0).
        _settings.MaxCaptionCount = value <= 5 ? 0 : (int)(Math.Round(value / 10.0) * 10);
    }

    [RelayCommand]
    private static async Task GoBack() => await Shell.Current.GoToAsync("..");
}
