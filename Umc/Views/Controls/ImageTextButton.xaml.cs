using System.Windows.Input;

namespace UC.Umc.Controls;

public partial class ImageTextButton : Grid
{
	public static readonly BindableProperty ImageProperty = BindableProperty.Create(nameof(Image), typeof(ImageSource), typeof(ImageTextButton));
	public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(ImageTextButton));
	public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(nameof(ImageSource), typeof(string), typeof(ImageTextButton));
	public static readonly BindableProperty ImageStyleProperty = BindableProperty.Create(nameof(ImageStyle), typeof(Style), typeof(ImageTextButton));
	public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ImageTextButton));
	public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ImageTextButton));
	public static readonly BindableProperty TintColorProperty = BindableProperty.Create(nameof(TintColor), typeof(Color), typeof(ImageTextButton), Colors.Black);

	public string Text
	{
		get { return (string)GetValue(TextProperty); }
		set { SetValue(TextProperty, value); }
	}
	public ImageSource Image
	{
		get { return (ImageSource)GetValue(ImageProperty); }
		set { SetValue(ImageProperty, value); }
	}
	public string ImageSource
	{
		get { return (string)GetValue(ImageSourceProperty); }
		set { SetValue(ImageSourceProperty, value); }
	}
	public Style ImageStyle
	{
		get { return (Style)GetValue(ImageStyleProperty); }
		set { SetValue(ImageStyleProperty, value); }
	}
	public ICommand Command
	{
		get { return (ICommand)GetValue(CommandProperty); }
		set { SetValue(CommandProperty, value); }
	}
	public object CommandParameter
	{
		get { return (object)GetValue(CommandParameterProperty); }
		set { SetValue(CommandParameterProperty, value); }
	}
	public Color TintColor
	{
		get { return (Color)GetValue(TintColorProperty); }
		set { SetValue(TintColorProperty, value); }
	}

    public ImageTextButton()
    {
        InitializeComponent();
    }
}