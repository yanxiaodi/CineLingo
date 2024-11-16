namespace CineLingo.Models;
public class Caption
{
    public string Text { get; set; } = string.Empty;

    public Caption()
    {
    }

    public Caption(string text)
    {
        Text = text;
    }
}
