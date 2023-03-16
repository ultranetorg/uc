namespace UC.Umc.Helpers;

public static class ColorHelper
{
    private static readonly Random rnd = new();

    public static Color GetRandomColor() => Color.FromRgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

    public static LinearGradientBrush CreateGradientColor(Color color) => new (
		new GradientStopCollection {
			new GradientStop(color, 0.1f),
			new GradientStop(color.WithSaturation(0.1f), 1.0f) });

    public static LinearGradientBrush CreateRandomGradientColor() => CreateGradientColor(GetRandomColor());
}
