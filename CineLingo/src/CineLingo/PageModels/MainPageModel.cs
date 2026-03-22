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

    // Colors assigned to speakers in the order they are first encountered.
    // Used for both diarization mode (keyed by speaker ID) and alternating mode (index cycles).
    private static readonly Color[] SpeakerColors =
    [
        Colors.WhiteSmoke,
        Color.FromArgb("#FFE066"),
        Color.FromArgb("#66E0FF"),
        Color.FromArgb("#FF9966"),
    ];

    // Maps speaker ID → color in English/diarization mode.
    private readonly Dictionary<string, Color> _speakerColorMap = new();
    // Alternating index used in multi-language mode where speaker IDs are unavailable.
    private int _alternatColorIndex;

    // Punctuation characters that indicate a sentence is already terminated.
    private static readonly char[] TerminatingPunctuation = ['.', '!', '?', '。', '！', '？', '…'];

    private readonly ISpeechCaptionService _speechCaptionService;

    public MainPageModel(ISpeechCaptionService speechCaptionService)
    {
        _speechCaptionService = speechCaptionService;
        _speechCaptionService.PartialResultReceived += OnPartialResultReceived;
        _speechCaptionService.FinalResultReceived += OnFinalResultReceived;
        _speechCaptionService.ErrorReceived += OnErrorReceived;
        _speechCaptionService.StatusChanged += OnStatusChanged;
    }

    [RelayCommand]
    private async Task StartTranscription()
    {
        IsStartEnabled = false;
        IsStopEnabled = true;
        try
        {
            await _speechCaptionService.StartAsync();
        }
        catch (Exception ex)
        {
            IsStartEnabled = true;
            IsStopEnabled = false;
            await Toast.Make($"Failed to start: {ex.Message}").Show(CancellationToken.None);
        }
    }

    [RelayCommand]
    private async Task StopTranscription()
    {
        IsStartEnabled = true;
        IsStopEnabled = false;
        PartialCaption = string.Empty;
        StatusMessage = string.Empty;
        _alternatColorIndex = 0;
        _speakerColorMap.Clear();
        await _speechCaptionService.StopAsync();
    }

    private void OnPartialResultReceived(object? sender, string text)
    {
        MainThread.BeginInvokeOnMainThread(() => PartialCaption = text);
    }

    private void OnFinalResultReceived(object? sender, FinalResultEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            PartialCaption = string.Empty;
            var color = ResolveColor(e.SpeakerId);
            var text = EnsureTerminatingPunctuation(e.Text);
            Captions.Add(new Caption(text, color, e.SpeakerId));
        });
    }

    private void OnErrorReceived(object? sender, string message)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            IsStartEnabled = true;
            IsStopEnabled = false;
            await Toast.Make(message).Show(CancellationToken.None);
        });
    }

    private void OnStatusChanged(object? sender, string status)
    {
        MainThread.BeginInvokeOnMainThread(() => StatusMessage = status);
    }

    /// <summary>
    /// Returns a color for the given speaker ID.
    /// Diarization mode: each unique speaker ID gets a stable assigned color.
    /// Multi-language mode (speakerId is null): colors alternate per utterance.
    /// </summary>
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

    /// <summary>Appends a period if the text doesn't already end with sentence-ending punctuation.</summary>
    private static string EnsureTerminatingPunctuation(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        return TerminatingPunctuation.Contains(text[^1]) ? text : text + ".";
    }
}

