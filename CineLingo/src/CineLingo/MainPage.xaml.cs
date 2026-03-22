using CineLingo.PageModels;

namespace CineLingo
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
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
    }
}
