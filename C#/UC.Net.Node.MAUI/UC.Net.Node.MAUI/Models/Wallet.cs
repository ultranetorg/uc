//using static UC.Net.Node.MAUI.Pages.RestoreAccountViewModel;

namespace UC.Net.Node.MAUI.Models;

public class Wallet:BindableObject
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Unts { get; set; }
    bool _isSelected;
    public bool IsSelected { get =>_isSelected; set { _isSelected = value; OnPropertyChanged(); } }
    public string IconCode { get; set; }
    Color _accountColor;
    public Color AccountColor { get => _accountColor; set { _accountColor = value; Background = GradientStops(value); OnPropertyChanged(); }}
    public LinearGradientBrush Background { set; get; }

    public LinearGradientBrush GradientStops(Color color)
    {
        return new LinearGradientBrush(
            new GradientStopCollection { 
				new GradientStop(color, 0.1f),
				new GradientStop(color.WithSaturation(0.7f), 1.0f) });
    }
}
