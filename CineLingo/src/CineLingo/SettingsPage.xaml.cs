using CineLingo.PageModels;

namespace CineLingo;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
