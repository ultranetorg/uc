namespace UO.Mobile.UUC.Models;

public record GradientColor
{
    public Color From { get; private set; }
    public Color To { get; private set; }

    public static GradientColor FromColors(Color from, Color to)
    {
        return new GradientColor
        {
            From = from,
            To = to,
        };
    }
}
