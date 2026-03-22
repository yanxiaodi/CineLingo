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

    // Two alternating colors to visually distinguish consecutive utterances.
    private static readonly Color[] SpeakerColors = [Colors.WhiteSmoke, Color.FromArgb("#FFE066")];
    private int _currentColorIndex;

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
            PartialCaption = string.Empty;
            HasPartialCaption = false;
            Captions.Add(new Caption(text, SpeakerColors[_currentColorIndex]));
            _currentColorIndex = 1 - _currentColorIndex;
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

