using CineLingo.Models;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;

namespace CineLingo.PageModels;

public partial class MainPageModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<Caption> _captions = [];

    [ObservableProperty] private Caption _currentCaption = new();

    [ObservableProperty] private bool _isStartEnabled = true;
    [ObservableProperty] private bool _isStopEnabled = false;

    private bool _isListening;
    private CancellationTokenSource _cancellationTokenSource;

    private readonly ISpeechToText _speechToText;

    public MainPageModel(ISpeechToText speechToText)
    {
        _speechToText = speechToText;
        CurrentCaption = new();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    [RelayCommand]
    private async Task StartTranscription()
    {
        if (!_isListening)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var isGranted = await _speechToText.RequestPermissions(_cancellationTokenSource.Token);
            if (!isGranted)
            {
                await Toast.Make("Permission not granted").Show(CancellationToken.None);
                return;
            }

            _isListening = true;
            IsStartEnabled = false;
            IsStopEnabled = true;
            while (_isListening)
            {
                try
                {
                    var recognitionResult = await _speechToText.ListenAsync(CultureInfo.CurrentCulture,
                        new Progress<string>(partialText => { CurrentCaption.Text = partialText + " "; }),
                        _cancellationTokenSource.Token);

                    if (recognitionResult.IsSuccessful)
                    {
                        CurrentCaption.Text = string.Empty;
                        Captions.Add(new Caption(recognitionResult.Text));
                    }
                    //else
                    //{
                    //    await Toast.Make(recognitionResult.Exception?.Message ?? "Unable to recognize speech")
                    //        .Show(CancellationToken.None);
                    //}
                }
                catch (TaskCanceledException)
                {
                    // Task was cancelled, this is expected
                    //await Toast.Make("Transcription stopped.").Show(CancellationToken.None);
                }
                catch (Exception ex)
                {
                    //await Toast.Make(ex.Message).Show(CancellationToken.None);
                }
            }
            //await StartListenAsync();
        }
    }

    [RelayCommand]
    private void StopTranscription()
    {
        _isListening = false;
        IsStartEnabled = true;
        IsStopEnabled = false;
        //await _cancellationTokenSource.CancelAsync();
    }

    //private async Task StartListenAsync()
    //{
    //    _speechToText.RecognitionResultUpdated -= OnRecognitionTextUpdated;
    //    _speechToText.RecognitionResultUpdated += OnRecognitionTextUpdated;
    //    _speechToText.RecognitionResultCompleted -= OnRecognitionTextCompleted;
    //    _speechToText.RecognitionResultCompleted += OnRecognitionTextCompleted;
    //    await _speechToText.StartListenAsync(CultureInfo.CurrentCulture, CancellationToken.None);
    //}

    //void OnRecognitionTextUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs args)
    //{
    //    CurrentCaption.Text += args.RecognitionResult;
    //}

    //async void OnRecognitionTextCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs args)
    //{
    //    Captions.Add(new Caption(args.RecognitionResult));
    //    CurrentCaption.Text = string.Empty;
    //    await Task.Delay(500);
    //    await StartListenAsync();
    //}
}
