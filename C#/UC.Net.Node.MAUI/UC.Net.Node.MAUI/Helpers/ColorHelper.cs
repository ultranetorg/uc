namespace UC.Net.Node.MAUI.Helpers;

public static class ColorHelper
{
    private static readonly Random rnd = new();

    public static Color GetRandomColor() => Color.FromRgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
}
