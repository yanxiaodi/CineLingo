using CommunityToolkit.Mvvm.ComponentModel;

namespace CineLingo.Models;
public partial class Caption : ObservableObject
{
    [ObservableProperty]
    public partial string Text { get; set; } = string.Empty;

    public Color TextColor { get; init; } = Colors.WhiteSmoke;

    public Caption() { }

    public Caption(string text, Color textColor)
    {
        Text = text;
        TextColor = textColor;
    }
}
