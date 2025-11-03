using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace UC.Umc.Controls;

// Obsolete
public class BorderEntry : Entry
{
	public static readonly BindableProperty AutoFocusProperty = BindableProperty.Create(nameof(AutoFocus), typeof(bool), typeof(BorderEntry), true);

	public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(BorderEntry), null);

	public static BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(BorderEntry), Colors.Transparent);

	public static BindableProperty FillColorProperty = BindableProperty.Create(nameof(FillColor), typeof(Color), typeof(BorderEntry), Colors.Transparent);

	public static BindableProperty BorderWidthProperty = BindableProperty.Create(nameof(BorderWidth), typeof(float), typeof(BorderEntry), 1f);

	public static BindableProperty BorderRadiusProperty = BindableProperty.Create(nameof(BorderRadius), typeof(float), typeof(BorderEntry), 1f);

	public static BindableProperty LeftPaddingProperty = BindableProperty.Create(nameof(LeftPadding), typeof(int), typeof(BorderEntry), 5);

	public static BindableProperty RightPaddingProperty = BindableProperty.Create(nameof(RightPadding), typeof(int), typeof(BorderEntry), 5);

	public static readonly BindableProperty NextEntryProperty = BindableProperty.Create(nameof(NextEntry), typeof(View), typeof(BorderEntry));

	public static readonly BindableProperty ImageProperty = BindableProperty.Create(nameof(Image), typeof(string), typeof(BorderEntry), string.Empty);

	public static readonly BindableProperty LineColorProperty = BindableProperty.Create(nameof(LineColor), typeof(Color), typeof(BorderEntry), Colors.Transparent);

	public bool AutoFocus
	{
		get { return (bool)GetValue(AutoFocusProperty); }
		set { SetValue(AutoFocusProperty, value); }
	}

	public ICommand Command
	{
		get { return (ICommand)GetValue(CommandProperty); }
		set { SetValue(CommandProperty, value); }
	}

	public Color BorderColor
	{
		get { return (Color)GetValue(BorderColorProperty); }
		set { SetValue(BorderColorProperty, value); }
	}

	public Color FillColor
	{
		get { return (Color)GetValue(FillColorProperty); }
		set { SetValue(FillColorProperty, value); }
	}

	public float BorderWidth
	{
		get { return (float)GetValue(BorderWidthProperty); }
		set { SetValue(BorderWidthProperty, value); }
	}

	public float BorderRadius
	{
		get { return (float)GetValue(BorderRadiusProperty); }
		set { SetValue(BorderRadiusProperty, value); }
	}

	public int LeftPadding
	{
		get { return (int)GetValue(LeftPaddingProperty); }
		set { SetValue(LeftPaddingProperty, value); }
	}

	public int RightPadding
	{
		get { return (int)GetValue(RightPaddingProperty); }
		set { SetValue(RightPaddingProperty, value); }
	}

	public View NextEntry
	{
		get => (View)GetValue(NextEntryProperty);
		set => SetValue(NextEntryProperty, value);
	}

	public Color LineColor
	{
		get { return (Color)GetValue(LineColorProperty); }
		set { SetValue(LineColorProperty, value); }
	}

    public BorderEntry() : base()
	{
	}

	protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        Completed += (sender, e) => NextEntry?.Focus();
    }
}
