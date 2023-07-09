using System.Windows.Input;
namespace UC.Umc.Controls;

public partial class SwitchView : ContentView
{
    private bool JustSet = true;
    private bool IsRunning = false;

	public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(double), typeof(SwitchView), default);

	public static readonly BindableProperty CurrentColorProperty = BindableProperty.Create(nameof(CurrentColor), typeof(Color), typeof(SwitchView), Colors.Gray);

	public static readonly BindableProperty OffColorProperty = BindableProperty.Create(nameof(OffColor), typeof(Color), typeof(SwitchView), Colors.Gray);

	public static readonly BindableProperty OnColorProperty = BindableProperty.Create(nameof(OnColor), typeof(Color), typeof(SwitchView), Colors.Blue);

	public static readonly BindableProperty IsOnProperty = BindableProperty.Create(nameof(IsOn), typeof(bool), typeof(SwitchView), true);

	public static readonly BindableProperty SwitchCommandProperty = BindableProperty.Create(nameof(SwitchCommand), typeof(ICommand), typeof(SwitchView));

	public double CornerRadius
	{
		get { return (double)GetValue(CornerRadiusProperty); }
		set { SetValue(CornerRadiusProperty, value); }
	}

	public Color CurrentColor
	{
		get { return (Color)GetValue(CurrentColorProperty); }
		set { SetValue(CurrentColorProperty, value); }
	}

	public Color OffColor
	{
		get { return (Color)GetValue(OffColorProperty); }
		set { SetValue(OffColorProperty, value); }
	}

	public Color OnColor
	{
		get { return (Color)GetValue(OnColorProperty); }
		set { SetValue(OnColorProperty, value); }
	}

	public bool IsOn
	{
		get { return (bool)GetValue(IsOnProperty); }
		set { SetValue(IsOnProperty, value); }
	}

	public ICommand SwitchCommand
	{
		get { return (ICommand)GetValue(SwitchCommandProperty); }
		set { SetValue(SwitchCommandProperty, value); }
	}

	public SwitchView()
    {
        InitializeComponent();
		Initialize();
	}

	public void Initialize()
	{
		CurrentColor = OffColor;
	}

	private async void OnSwitchedAsync(object sender, EventArgs e)
	{
		await SwitchAsync();
	}

	private async void OnSizeChangedAsync(object sender, EventArgs e)
	{
		if (frame.Width > 0)
		{
			frame.WidthRequest = frame.Height * 2;
			thumb.HeightRequest = thumb.WidthRequest = frame.Height - 4;
			thumb.HorizontalOptions = LayoutOptions.Start;
			await SwitchAsync();
		}
	}

	private async Task SwitchAsync()
	{
		if (!IsRunning)
		{
			IsRunning = true;

			if (IsOn && !JustSet)
			{
				await thumb.TranslateTo(thumb.TranslationX - thumb.Width, 0);
				CurrentColor = OffColor;
			}
			else if (!IsOn)
			{
				await thumb.TranslateTo(thumb.Width, 0);
				CurrentColor = OnColor;
			}
			IsOn = !IsOn;
			IsRunning = JustSet = false;
		}
	}
}