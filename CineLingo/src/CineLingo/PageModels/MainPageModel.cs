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
    [ObservableProperty] public partial bool HasPartialCaption { get; set; }
    [ObservableProperty] public partial bool IsStartEnabled { get; set; } = true;
    [ObservableProperty] public partial bool IsStopEnabled { get; set; }

    // Two alternating colors to visually hint at speaker changes during rapid dialogue.
    private static readonly Color[] SpeakerColors = [Colors.WhiteSmoke, Color.FromArgb("#FFE066")];
    private int _currentColorIndex;
    private DateTime _lastFinalResultTime = DateTime.MinValue;
    private const double SpeakerSwitchThresholdSeconds = 1.5;

    private readonly ISpeechCaptionService _speechCaptionService;

    public MainPageModel(ISpeechCaptionService speechCaptionService)
    {
        _speechCaptionService = speechCaptionService;
        _speechCaptionService.PartialResultReceived += OnPartialResultReceived;
        _speechCaptionService.FinalResultReceived += OnFinalResultReceived;
        _speechCaptionService.ErrorReceived += OnErrorReceived;
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
        HasPartialCaption = false;
        _currentColorIndex = 0;
        _lastFinalResultTime = DateTime.MinValue;
        await _speechCaptionService.StopAsync();
    }

    private void OnPartialResultReceived(object? sender, string text)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            PartialCaption = text;
            HasPartialCaption = !string.IsNullOrEmpty(text);
        });
    }

    private void OnFinalResultReceived(object? sender, string text)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var now = DateTime.UtcNow;
            var gap = (now - _lastFinalResultTime).TotalSeconds;

            // Switch speaker color when utterances come quickly (rapid dialogue between speakers).
            if (_lastFinalResultTime != DateTime.MinValue && gap < SpeakerSwitchThresholdSeconds)
                _currentColorIndex = 1 - _currentColorIndex;

            _lastFinalResultTime = now;
            PartialCaption = string.Empty;
            HasPartialCaption = false;
            Captions.Add(new Caption(text, SpeakerColors[_currentColorIndex]));
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
}

