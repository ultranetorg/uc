namespace UC.Net.Node.MAUI.Models;

public class Wallet : BindableObject
{
    private Color _accountColor;
    private bool _isSelected;

    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Unts { get; set; }
    public string IconCode { get; set; }

    public bool IsSelected { 
		get => _isSelected;
		set { 
			_isSelected = value; 
			OnPropertyChanged(); 
		}
	}

    public Color AccountColor { 
		get => _accountColor;
		set { 
			_accountColor = value; 
			Background = GradientStops(value); 
			OnPropertyChanged(); 
		} 
	}

    public LinearGradientBrush Background { set; get; }

    public static LinearGradientBrush GradientStops(Color color) => new (
		new GradientStopCollection { 
			new GradientStop(color, 0.1f),
			new GradientStop(color.WithSaturation(0.7f), 1.0f) });
}
