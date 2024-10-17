namespace UC.Umc.Models.Common;

public record GradientColor
{
	public Color From { get; private set; }
	public Color To { get; private set; }

	public static GradientColor FromColors(Color from, Color to) => new(){ From = from, To = to };
}
