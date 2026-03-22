using CineLingo.Models;
using CineLingo.Services.Interfaces;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CineLingo.PageModels;

public partial class MainPageModel : ObservableObject
{
    [ObservableProperty] public partial ObservableCollection<Caption> Captions { get; set; } = [];
    [ObservableProperty] public partial string PartialCaption { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsStartEnabled { get; set; } = true;
    [ObservableProperty] public partial bool IsStopEnabled { get; set; }
    [ObservableProperty] public partial string StatusMessage { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsTranslationEnabled { get; set; }
    public bool HasStatusMessage => !string.IsNullOrEmpty(StatusMessage);

    /// <summary>
    /// Raised after a translation result is applied to a caption, so the view
    /// can scroll to ensure the extra subtitle line is not hidden behind the bottom panel.
    /// </summary>
    public event EventHandler? ScrollToLastRequested;

    /// <summary>Opacity for the translation toggle button: full when on, dimmed when off.</summary>
    public double TranslationOpacity => IsTranslationEnabled ? 1.0 : 0.35;
    private static readonly Color[] SpeakerColors =
    [
        Colors.WhiteSmoke,
        Color.FromArgb("#FFE066"),
        Color.FromArgb("#66E0FF"),
        Color.FromArgb("#FF9966"),
    ];

    private readonly Dictionary<string, Color> _speakerColorMap = new();
    private int _alternatColorIndex;
    private static readonly char[] TerminatingPunctuation = ['.', '!', '?', '。', '！', '？', '…'];

    private readonly ISpeechCaptionService _speechCaptionService;
    private readonly ITranslationService _translationService;
    private readonly AppSettings _appSettings;

    public MainPageModel(ISpeechCaptionService speechCaptionService,
                         ITranslationService translationService,
                         AppSettings appSettings)
    {
        _speechCaptionService = speechCaptionService;
        _translationService   = translationService;
        _appSettings          = appSettings;

        IsTranslationEnabled = appSettings.IsTranslationEnabled;

        _speechCaptionService.PartialResultReceived += OnPartialResultReceived;
        _speechCaptionService.FinalResultReceived   += OnFinalResultReceived;
        _speechCaptionService.ErrorReceived         += OnErrorReceived;
        _speechCaptionService.StatusChanged         += OnStatusChanged;
    }

    partial void OnIsTranslationEnabledChanged(bool value)
    {
        _appSettings.IsTranslationEnabled = value;
        OnPropertyChanged(nameof(TranslationOpacity));
    }

    [RelayCommand]
    private void ToggleTranslation() => IsTranslationEnabled = !IsTranslationEnabled;

    [RelayCommand]
    private static async Task NavigateToSettings() => await Shell.Current.GoToAsync("settings");

    [RelayCommand]
    private async Task StartTranscription()
    {
        IsStartEnabled = false;
        IsStopEnabled  = true;
        try { await _speechCaptionService.StartAsync(); }
        catch (Exception ex)
        {
            IsStartEnabled = true;
            IsStopEnabled  = false;
            await Toast.Make($"Failed to start: {ex.Message}").Show(CancellationToken.None);
        }
    }

    [RelayCommand]
    private async Task StopTranscription()
    {
        IsStartEnabled = true;
        IsStopEnabled  = false;
        PartialCaption = string.Empty;
        StatusMessage  = string.Empty;
        _alternatColorIndex = 0;
        _speakerColorMap.Clear();
        await _speechCaptionService.StopAsync();
    }

    private void OnPartialResultReceived(object? sender, string text) =>
        MainThread.BeginInvokeOnMainThread(() => PartialCaption = text);

    private void OnFinalResultReceived(object? sender, FinalResultEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            PartialCaption = string.Empty;
            var caption = new Caption(EnsureTerminatingPunctuation(e.Text),
                                      ResolveColor(e.SpeakerId),
                                      e.SpeakerId);
            Captions.Add(caption);

            if (_appSettings.MaxCaptionCount > 0 && Captions.Count > _appSettings.MaxCaptionCount)
                Captions.RemoveAt(0);

            if (IsTranslationEnabled)
                _ = TranslateCaptionAsync(caption, e.DetectedLanguage);
        });
    }

    private void OnErrorReceived(object? sender, string message) =>
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            IsStartEnabled = true;
            IsStopEnabled  = false;
            await Toast.Make(message).Show(CancellationToken.None);
        });

    private void OnStatusChanged(object? sender, string status) =>
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusMessage = status;
            OnPropertyChanged(nameof(HasStatusMessage));
        });

    /// <summary>
    /// Calls the translation service asynchronously and updates the caption's
    /// TranslatedText when the result arrives. The UI refreshes automatically
    /// via the ObservableProperty binding.
    /// </summary>
    private async Task TranslateCaptionAsync(Caption caption, string? detectedLanguage)
    {
        try
        {
            var translated = await _translationService.TranslateAsync(
                caption.Text, detectedLanguage, _appSettings.TargetLanguage);

            if (!string.IsNullOrWhiteSpace(translated))
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    caption.TranslatedText = translated;
                    ScrollToLastRequested?.Invoke(this, EventArgs.Empty);
                });
        }
        catch
        {
            // Translation failure is non-critical; original text remains visible.
        }
    }

    private Color ResolveColor(string? speakerId)
    {
        if (speakerId is null)
        {
            var color = SpeakerColors[_alternatColorIndex % SpeakerColors.Length];
            _alternatColorIndex++;
            return color;
        }
        if (!_speakerColorMap.TryGetValue(speakerId, out var speakerColor))
        {
            speakerColor = SpeakerColors[_speakerColorMap.Count % SpeakerColors.Length];
            _speakerColorMap[speakerId] = speakerColor;
        }
        return speakerColor;
    }

    private static string EnsureTerminatingPunctuation(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        return TerminatingPunctuation.Contains(text[^1]) ? text : text + ".";
    }
}

