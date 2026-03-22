using CineLingo.PageModels;
using System.ComponentModel;

namespace CineLingo
{
    public partial class MainPage : ContentPage
    {
        private CancellationTokenSource? _animCts;

        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
            model.PropertyChanged      += OnModelPropertyChanged;
            model.ScrollToLastRequested += OnScrollToLastRequested;
        }

        private void OnScrollToLastRequested(object? sender, EventArgs e)
        {
            var model = BindingContext as MainPageModel;
            var lastItem = model?.Captions?.LastOrDefault();
            if (lastItem != null)
                CaptionsView.ScrollTo(lastItem, position: ScrollToPosition.End, animate: true);
        }

        private void OnBottomPanelSizeChanged(object sender, EventArgs e)
        {
            // Grid Row 0 (*) automatically shrinks when Row 1 (Auto) grows, but CollectionView
            // does not re-scroll on container resize. Explicitly scroll to the last item so
            // captions are never hidden behind the bottom panel.
            var model = BindingContext as MainPageModel;
            var lastItem = model?.Captions?.LastOrDefault();
            if (lastItem != null)
                CaptionsView.ScrollTo(lastItem, position: ScrollToPosition.End, animate: false);
        }

        private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(MainPageModel.IsStopEnabled)) return;

            if (((MainPageModel)sender!).IsStopEnabled)
            {
                _animCts?.Cancel();
                _animCts = new CancellationTokenSource();
                _ = RunListeningAnimation(_animCts.Token);
            }
            else
            {
                _animCts?.Cancel();
                ListeningDot.Scale = 1.0;
                ListeningDot.Opacity = 1.0;
            }
        }

        /// <summary>
        /// Pulse the listening dot: scale up + fade out, then back down + fade in, in a loop.
        /// </summary>
        private async Task RunListeningAnimation(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.WhenAll(
                    ListeningDot.ScaleToAsync(1.5, 800, Easing.SinInOut),
                    ListeningDot.FadeToAsync(0.3, 800, Easing.SinInOut));

                if (token.IsCancellationRequested) break;

                await Task.WhenAll(
                    ListeningDot.ScaleToAsync(1.0, 800, Easing.SinInOut),
                    ListeningDot.FadeToAsync(1.0, 800, Easing.SinInOut));
            }
        }
    }
}
