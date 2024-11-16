using CineLingo.Models;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;

namespace CineLingo.PageModels;
public partial class MainPageModel(ISpeechToText speechToText) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Caption> _captions = [];

    [ObservableProperty]
    private Caption _currentCaption = new();

    [RelayCommand]
    private async Task StartTranscription(CancellationToken cancellationToken)
    {
        var isGranted = await speechToText.RequestPermissions(cancellationToken);
        if (!isGranted)
        {
            await Toast.Make("Permission not granted").Show(CancellationToken.None);
            return;
        }

        await StartListenAsync();

    }

    private async Task StartListenAsync()
    {
        speechToText.RecognitionResultUpdated -= OnRecognitionTextUpdated;
        speechToText.RecognitionResultUpdated += OnRecognitionTextUpdated;
        speechToText.RecognitionResultCompleted -= OnRecognitionTextCompleted;
        speechToText.RecognitionResultCompleted += OnRecognitionTextCompleted;
        await speechToText.StartListenAsync(CultureInfo.CurrentCulture, CancellationToken.None);
    }

    void OnRecognitionTextUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs args)
    {
        CurrentCaption.Text += args.RecognitionResult;
        OnPropertyChanged(nameof(CurrentCaption));
    }

    async void OnRecognitionTextCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs args)
    {
        Captions.Add(new Caption(args.RecognitionResult));
        CurrentCaption.Text = string.Empty;
        OnPropertyChanged(nameof(CurrentCaption));
        await Task.Delay(500);
        await StartListenAsync();
    }

}
