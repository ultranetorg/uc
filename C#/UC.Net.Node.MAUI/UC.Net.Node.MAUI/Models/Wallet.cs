namespace UC.Net.Node.MAUI.Models;

public partial class Wallet : ObservableObject
{
    private Color _accountColor;

    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Unts { get; set; }
    public string IconCode { get; set; }

    public bool IsSelected { get; set; }

	// lets say 1 unts = $1 unless we can recieve rate
	public string DisplayAmount => $"{Unts} UNT (${Unts})";

    public Color AccountColor {
		get => _accountColor;
		set {
			_accountColor = value;
			Background = GradientStops(value);
		}
	}
	
	[ObservableProperty]
    public LinearGradientBrush _background;

	// TODO: probably need to be moved into view model and need to remove inheritance
    public static LinearGradientBrush GradientStops(Color color) => new (
		new GradientStopCollection {
			new GradientStop(color, 0.1f),
			new GradientStop(color.WithSaturation(0.1f), 1.0f) });
}
