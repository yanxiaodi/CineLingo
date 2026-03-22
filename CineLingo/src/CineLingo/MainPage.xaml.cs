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
            CaptionsView.Margin = new Thickness(0, 0, 0, BottomPanel.Height);
        }
    }
}
