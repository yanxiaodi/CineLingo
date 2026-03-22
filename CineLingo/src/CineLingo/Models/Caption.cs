using CommunityToolkit.Mvvm.ComponentModel;

namespace CineLingo.Models;
public partial class Caption : ObservableObject
{
    [ObservableProperty]
    public partial string Text { get; set; } = string.Empty;

    public Caption()
    {
    }

    public Caption(string text)
    {
        Text = text;
    }
}
