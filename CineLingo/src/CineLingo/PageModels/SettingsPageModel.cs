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

    public static readonly IReadOnlyList<double>  FontSizeValues   = [14.0, 18.0, 22.0, 26.0];
    public static readonly IReadOnlyList<string>  FontSizeLabels   = ["Small (14)", "Medium (18)", "Large (22)", "X-Large (26)"];
    public static readonly IReadOnlyList<int>     MaxCaptionValues = [50, 100, 200, 0];
    public static readonly IReadOnlyList<string>  MaxCaptionLabels = ["50", "100", "200", "Unlimited"];

    private readonly AppSettings _settings;

    [ObservableProperty] public partial bool           IsTranslationEnabled     { get; set; }
    [ObservableProperty] public partial LanguageOption SelectedLanguage         { get; set; }
    [ObservableProperty] public partial int            SelectedFontSizeIndex    { get; set; }
    [ObservableProperty] public partial int            SelectedMaxCaptionIndex  { get; set; }
    [ObservableProperty] public partial bool           EnableSpeakerDiarization { get; set; }

    public SettingsPageModel(AppSettings settings)
    {
        _settings = settings;

        IsTranslationEnabled    = settings.IsTranslationEnabled;
        SelectedLanguage        = SupportedLanguages.FirstOrDefault(l => l.Code == settings.TargetLanguage)
                                  ?? SupportedLanguages[0];
        var fsi = FontSizeValues.ToList().IndexOf(settings.CaptionFontSize);
        SelectedFontSizeIndex   = fsi >= 0 ? fsi : 1;
        var mci = MaxCaptionValues.ToList().IndexOf(settings.MaxCaptionCount);
        SelectedMaxCaptionIndex = mci >= 0 ? mci : 1;
        EnableSpeakerDiarization = settings.EnableSpeakerDiarization;
    }

    partial void OnIsTranslationEnabledChanged(bool value)        => _settings.IsTranslationEnabled = value;
    partial void OnSelectedLanguageChanged(LanguageOption value) { if (value != null) _settings.TargetLanguage = value.Code; }

    partial void OnSelectedFontSizeIndexChanged(int value)
    {
        if (value >= 0 && value < FontSizeValues.Count)
        {
            _settings.CaptionFontSize = FontSizeValues[value];
            // Update DynamicResource so existing captions resize immediately.
            if (Application.Current?.Resources is not null)
                Application.Current.Resources["CaptionFontSize"] = FontSizeValues[value];
        }
    }

    partial void OnSelectedMaxCaptionIndexChanged(int value)
    {
        if (value >= 0 && value < MaxCaptionValues.Count)
            _settings.MaxCaptionCount = MaxCaptionValues[value];
    }

    partial void OnEnableSpeakerDiarizationChanged(bool value) => _settings.EnableSpeakerDiarization = value;

    [RelayCommand]
    private static async Task GoBack() => await Shell.Current.GoToAsync("..");
}
