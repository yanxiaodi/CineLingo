using CommunityToolkit.Mvvm.ComponentModel;

namespace CineLingo.Models;
public partial class Caption : ObservableObject
{
    [ObservableProperty]
    private string _text = string.Empty;

    public Caption()
    {
    }

    public Caption(string text)
    {
        Text = text;
    }
}
